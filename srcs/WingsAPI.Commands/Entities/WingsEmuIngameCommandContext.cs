// NosEmu
// 


using System;
using Qmmands;
using WingsEmu.Game.Networking;

namespace WingsEmu.Commands.Entities
{
    public sealed class WingsEmuIngameCommandContext : CommandContext
    {
        public WingsEmuIngameCommandContext(string message, IClientSession sender, string prefix, CommandService cmds, IServiceProvider services) : base(services)
        {
            CommandService = cmds;

            Message = message;
            Player = sender;
            Prefix = prefix;

            int pos = message.IndexOf(prefix, StringComparison.Ordinal) + 1;
            Input = message.Substring(pos);
        }

        public CommandService CommandService { get; }

        public Command Command { get; set; }

        public string Message { get; set; }

        public string Prefix { get; set; }
        public IClientSession Player { get; set; }

        public string Input { get; set; }
    }
}