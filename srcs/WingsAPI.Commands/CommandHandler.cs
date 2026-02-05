// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.Commands.Interfaces;
using WingsEmu.DTOs.Account;
using WingsEmu.Game;
using WingsEmu.Game.Commands;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Commands
{
    /* todo: find a better way to deal with TAP in world and here.
     *       handle errors correctly and return them to the user ingame.
     */
    public class CommandHandler : ICommandContainer, IGlobalCommandExecutor
    {
        private readonly CommandService _commands;

        /// <summary>
        ///     This class should be instanciated with our Container.
        /// </summary>
        public CommandHandler(IServiceProvider provider)
        {
            _commands = new CommandService(new CommandServiceConfiguration
            {
                StringComparison = StringComparison.OrdinalIgnoreCase
            });

            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandExecutionFailed += Commands_CommandExecutionFailed;

            Services = provider;
        }

        public IServiceProvider Services { get; }

        public void AddModule<T>() where T : SaltyModuleBase
        {
            Log.Info($"[ADD_MODULE] {typeof(T).Name}");
            _commands.AddModule<T>();

            IReadOnlyList<Command> readOnlyList = _commands.GetAllModules().FirstOrDefault(s => s.Type == typeof(T))?.Commands;
            if (readOnlyList != null)
            {
                foreach (Command command in readOnlyList)
                {
                    Log.Info($"[ADD_COMMAND] {command}");
                }
            }
        }

        public void RemoveModule<T>() where T : SaltyModuleBase
        {
            Module module = _commands.GetAllModules().FirstOrDefault(s => s.Type == typeof(T));

            if (module is null)
            {
                throw new ArgumentException("The given module is not registered in the command container.");
            }

            _commands.RemoveModule(module);
        }

        public Module[] GetModulesByName(string name, bool caseSensitive = true)
        {
            return _commands.GetAllModules().Where(x => caseSensitive ? x.Name == name : x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        public Command[] GetCommandsByName(string name, bool caseSensitive = true)
        {
            return _commands.GetAllCommands().Where(x => caseSensitive ? x.Name == name : x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        public void AddTypeParser<T>(TypeParser<T> typeParser)
        {
            _commands.AddTypeParser(typeParser);
            Log.Info($"[ADD_TYPE_PARSER] {typeParser.GetType().Name}");
        }

        /// <inheritdoc />
        /// <summary>
        ///     This is where every message from the InGame tchat starting with our prefix will arrive.
        ///     In our case, the parameter message represents the raw message sent by the user.
        ///     The parameter of type object would represent the instance of the entity that invoked the command.
        ///     That method could be called on each messages sent in the in-game tchat. We will just check that it starts with our
        ///     prefix ($).
        ///     Then we will create a Context that will propagate onto the command.
        ///     The CommandService will try to parse the message and find a command with the parsed arguments and will perform some
        ///     checks, if necessary.
        /// </summary>
        /// <param name="message">It represents the already parsed command with its parameters.</param>
        /// <param name="entity">It represents the instance of the entity that performed the action of sending a message.</param>
        /// <param name="prefix"></param>
        public async Task HandleMessageAsync(string message, object entity, string prefix)
        {
            if (entity is not IClientSession player)
            {
                return;
            }

            if (!player.HasSelectedCharacter)
            {
                return;
            }

            if (!player.HasCurrentMapInstance)
            {
                return;
            }

            var ctx = new WingsEmuIngameCommandContext(message, player, prefix, _commands, Services);

            IResult result = await _commands.ExecuteAsync(ctx.Input, ctx);
            if (result.IsSuccessful)
            {
                ctx.Command = (result as CommandResult)?.Command;
                var authorityAttribute = (RequireAuthorityAttribute)ctx.Command?.Module?.Checks.FirstOrDefault(check => check is RequireAuthorityAttribute);
                if (authorityAttribute == null || authorityAttribute.Authority < AuthorityType.GM)
                {
                    ctx.Player.PlayerEntity.Session.EmitEvent(new PlayerCommandEvent
                    {
                        Command = ctx.Message
                    });
                    return;
                }

                ctx.Player.PlayerEntity.Session.EmitEvent(new GmCommandEvent
                {
                    Command = ctx.Message,
                    PlayerAuthority = ctx.Player.Account.Authority,
                    CommandAuthority = authorityAttribute.Authority
                });
                return;
            }

            await HandleErrorAsync(result, ctx);
        }

        public void HandleCommand(string command, IClientSession sender, string prefix)
        {
            HandleCommandAsync(command, sender, prefix).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task HandleCommandAsync(string command, IClientSession sender, string prefix)
        {
            await HandleMessageAsync(command, sender, prefix);
        }

        /// <summary>
        ///     This event is being invoked when the excecuted of a command threw an exception.
        ///     Error results are handled by the result of CommandService#ExecuteAsync.
        /// </summary>
        /// <param name="result">Result with its associated exception.</param>
        /// <param name="context">It represents the context. Must be casted to our custom context (WingsEmuIngameCommandContext)</param>
        /// <param name="services"></param>
        /// <returns></returns>
        private ValueTask Commands_CommandExecutionFailed(object sender, CommandExecutionFailedEventArgs e)
        {
            switch (e.Result.Exception)
            {
                default:
                    Log.Debug($"{e.Result.Exception.GetType()} occured.\nError message: {e.Result.Exception.Message}.\nStack trace: {e.Result.Exception.StackTrace}");
                    break;
            }

            return default;
        }

        /// <summary>
        ///     This event is being invoked when the execution of a command is over. When the command returned a result.
        ///     It could be a custom result that we can cast from our instance of CommandResult.
        /// </summary>
        /// <param name="command">It represents the command that has been executed.</param>
        /// <param name="result">
        ///     It represents the returned result. It can an 'empty' result when the command returned a Task, or a
        ///     custom result.
        /// </param>
        /// <param name="context">It represents the context. Must be casted to our custom context (WingsEmuIngameCommandContext)</param>
        /// <returns></returns>
        private ValueTask Commands_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            if (e.Context is not WingsEmuIngameCommandContext ctx)
            {
                Log.Debug($"Command context: {e.Context.GetType()}. This is bad. Please report this.");
                return default;
            }

            Log.Debug($"The command {e.Context.Command.Name} (from player {ctx.Player.PlayerEntity.Name} [{ctx.Player.PlayerEntity.Id}]) has successfully been executed.");

            if (e.Result is SaltyCommandResult res && !string.IsNullOrWhiteSpace(res.Message))
            {
                ctx.Player.SendChatMessage("[COMMAND] " + res.Message, e.Result.IsSuccessful ? ChatMessageColorType.Green : ChatMessageColorType.Red);
            }
            return default;
        }

        /// <summary>
        ///     This is being called when the CommandService returned an unsuccessfull result.
        /// </summary>
        /// <param name="result">This represents the generic result returned by the command service. We'll check what was wrong.</param>
        /// <param name="ctx">This represents our context for this result.</param>
        private async Task HandleErrorAsync(IResult result, WingsEmuIngameCommandContext ctx)
        {
            Log.Debug($"An error occured: {result}");

            var errorBuilder = new StringBuilder();
            bool help = false;

            switch (result)
            {
                case ChecksFailedResult ex:
                    ctx.Command = ex.Command;
                    Log.Debug("Some checks have failed: " + string.Join("\n", ex.FailedChecks.Select(x => x.Result)));
                    break;
                case TypeParseFailedResult ex:
                    ctx.Command = ex.Parameter.Command;
                    errorBuilder.Append(ex.FailureReason);
                    help = true;
                    break;
                case CommandNotFoundResult ex:
                    errorBuilder.Append($"The command was not found: {ctx.Input}");
                    break;
                case ArgumentParseFailedResult ex:
                    ctx.Command = ex.Command;
                    errorBuilder.Append(ex.Command.Parameters == null ? "Too many arguments." : $"The argument for the parameter {ex.Command.Name} was invalid.");
                    help = true;
                    break;
                case SaltyCommandResult ex:
                    ctx.Command = ex.Command;
                    errorBuilder.Append($"{ctx.Command.Name}: {ex.Message}");
                    break;
                case OverloadsFailedResult ex:
                    ctx.Command = ex.FailedOverloads.Select(x => x.Key).FirstOrDefault();
                    Log.Debug($"Every overload failed: {string.Join("\n", ex.FailedOverloads.Select(x => x.Value.FailureReason))}");
                    errorBuilder.Append("Your command syntax was wrong.");
                    help = true;
                    break;
            }

            if (errorBuilder.Length == 0)
            {
                return;
            }

            ctx.Player.SendChatMessage(errorBuilder.ToString(), ChatMessageColorType.Red);
            if (help)
            {
                await _commands.ExecuteAsync($"help {ctx.Command.FullAliases[0]}", ctx);
            }
        }
    }
}