// NosEmu
// 


using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Commands.TypeParsers
{
    public sealed class PlayerEntityTypeParser : TypeParser<IClientSession>
    {
        private readonly ISessionManager _sessionManager;

        public PlayerEntityTypeParser(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public override ValueTask<TypeParserResult<IClientSession>> ParseAsync(Parameter param, string value, CommandContext context)
        {
            IClientSession player = _sessionManager.GetSessionByCharacterName(value);

            return player is null
                ? new ValueTask<TypeParserResult<IClientSession>>(new TypeParserResult<IClientSession>($"Player {value} is not connected or doesn't exist."))
                : new ValueTask<TypeParserResult<IClientSession>>(new TypeParserResult<IClientSession>(player));
        }
    }
}