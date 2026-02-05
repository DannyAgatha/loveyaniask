// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Entities;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Help;

[Name("help")] //todo: replace skip/take by proper 48 char limit paginator
[Description("Module related to help.")]
public class HelpModule : SaltyModuleBase
{
    private readonly IGameLanguageService _gamelanguage;

    public HelpModule(IGameLanguageService gamelanguage) => _gamelanguage = gamelanguage;

    [Command("help")]
    public async Task HelpAsync()
    {
        var modules = Context.CommandService.GetAllModules().Where(x => x.Aliases.Count > 0 && x.Commands.Count > 0 && x.Parent == null).ToList();
        IEnumerable<Command> modulelessCommands = Context.CommandService.GetAllModules().Where(x => x.Aliases.Count == 0).SelectMany(x => x.Commands);

        foreach (Module module in modules.ToList())
        {
            IResult result = await module.RunChecksAsync(Context);
            if (!result.IsSuccessful)
            {
                modules.Remove(module);
            }
        }

        var commands = new List<Command>();
        foreach (Command command in modulelessCommands.ToList().Where(command => commands.All(x => x.FullAliases[0] != command.FullAliases[0])).OrderBy(s => s.Name[0]))
        {
            IResult result = await command.RunChecksAsync(Context);
            if (result.IsSuccessful)
            {
                commands.Add(command);
            }
        }

        commands = commands.GroupBy(s => s.Name).Select(s => s.First()).ToList();
        Context.Player.SendChatMessage(_gamelanguage.GetLanguage(GameDialogKey.COMMANDS_CHATMESSAGE_HELP, Context.Player.UserLanguage), ChatMessageColorType.Red);
        Context.Player.SendChatMessage(_gamelanguage.GetLanguage(GameDialogKey.COMMANDS_CHATMESSAGE_AVAILABLE, Context.Player.UserLanguage), ChatMessageColorType.Red);
        for (int i = 0; i < commands.Count / 6 + 1; i++)
        {
            Context.Player.SendChatMessage(" -> " + string.Join(", ", commands.Skip(i * 6).Take(6).Select(x => x.Name)), ChatMessageColorType.Green);
        }
    }

    [Command("help")]
    public async Task HelpAsync([Remainder] string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            await HelpAsync();
            return;
        }

        var cmds = Context.CommandService.FindCommands(command).ToList();

        foreach (CommandMatch cmd in cmds.ToList())
        {
            IResult result = await cmd.Command.RunChecksAsync(Context);
            if (!result.IsSuccessful)
            {
                cmds.Remove(cmd);
            }
        }

        if (cmds.Count == 0)
        {
            Module module = Context.CommandService.TopLevelModules.FirstOrDefault(x => x.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
            //Module module = Context.CommandService.FindModules(command).FirstOrDefault()?.Module;

            IResult passCheck = await module?.RunChecksAsync(Context);

            if (module is null || !passCheck.IsSuccessful)
            {
                var cmdArgs = command.Split(' ').ToList();
                cmdArgs.RemoveAt(cmdArgs.Count - 1);

                await HelpAsync(string.Join(" ", cmdArgs));
                return;
            }

            Context.Player.SendChatMessage($"Help: ({command})", ChatMessageColorType.Red);
            if (module.Submodules.Count > 0)
            {
                Context.Player.SendChatMessage("Submodules:", ChatMessageColorType.Red);
                for (int i = 0; i < module.Submodules.Count / 6 + 1; i++)
                {
                    Context.Player.SendChatMessage(" -> " + string.Join(", ", module.Submodules.Skip(i * 6).Take(6).Select(x => $"{x.Aliases[0]}")),
                        ChatMessageColorType.Green);
                }
            }

            if (module.Commands.Count > 0)
            {
                Context.Player.SendChatMessage("Commands:", ChatMessageColorType.Red);
                for (int i = 0; i < module.Commands.Count / 6 + 1; i++)
                {
                    Context.Player.SendChatMessage(" -> " + string.Join(", ", module.Commands.Skip(i * 6).Take(6).Select(x => $"{x.Aliases[0]}")), ChatMessageColorType.Green);
                }
            }
        }

        Context.Player.SendChatMessage(_gamelanguage.GetLanguage(GameDialogKey.COMMANDS_CHATMESSAGE_USAGES, Context.Player.UserLanguage), ChatMessageColorType.Red);
        foreach (CommandMatch cmd in cmds)
        {
            Context.Player.SendChatMessage(
                $"${cmd.Command.Name} {string.Join(" ", cmd.Command.Parameters.Select(x => x.IsOptional ? $"<{x.Name}>" : $"[{x.Name}]"))}".ToLowerInvariant(), ChatMessageColorType.Green);
            foreach (Parameter param in cmd.Command.Parameters)
            {
                string str = "";

                str = param.IsOptional ? $"<{param.Name}>:" : $"[{param.Name}]:";

                str += $" {param.Description ?? _gamelanguage.GetLanguage(GameDialogKey.COMMANDS_CHATMESSAGE_UNDOCUMENTED, Context.Player.UserLanguage)}";
                Context.Player.SendChatMessage(str, ChatMessageColorType.Green);
            }

            Context.Player.SendChatMessage(cmd.Command.Description ?? _gamelanguage.GetLanguage(GameDialogKey.COMMANDS_CHATMESSAGE_UNDOCUMENTED, Context.Player.UserLanguage),
                ChatMessageColorType.Red);
            Context.Player.SendChatMessage(" ", ChatMessageColorType.Red);
        }
    }
}