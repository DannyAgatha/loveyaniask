using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.Command
{
    [Name("Rainbow Battle")]
    [Group("rainbowbattle", "rbb")]
    [RequireAuthority(AuthorityType.Owner)]
    public class RainbowBattleCommandModule : SaltyModuleBase
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;

        public RainbowBattleCommandModule(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

        [Command("start")]
        public async Task<SaltyCommandResult> Start()
        {
            await Context.Player.EmitEventAsync(new RainbowBattleStartEvent
            {
                RedTeam = new List<IClientSession>
                {
                    Context.Player
                },
                BlueTeam = new List<IClientSession>()
            });

            return new SaltyCommandResult(true);
        }

        [Command("end")]
        public async Task<SaltyCommandResult> End()
        {
            if (!Context.Player.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
            {
                return new SaltyCommandResult(false);
            }

            await _asyncEventPipeline.ProcessEventAsync(new RainbowBattleEndEvent
            {
                RainbowBattleParty = Context.Player.PlayerEntity.RainbowBattleComponent.RainbowBattleParty
            });
            return new SaltyCommandResult(true);
        }

        [Command("register")]
        public async Task<SaltyCommandResult> Register()
        {
            await Context.Player.EmitEventAsync(new RainbowBattleStartRegisterEvent());

            return new SaltyCommandResult(true);
        }
    }
}