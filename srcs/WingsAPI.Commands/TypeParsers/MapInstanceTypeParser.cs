using System;
using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Game.Maps;

namespace WingsEmu.Commands.TypeParsers
{
    public sealed class MapInstanceTypeParser : TypeParser<IMapInstance>
    {
        private readonly IMapManager _mapManager;

        public MapInstanceTypeParser(IMapManager mapManager) => _mapManager = mapManager;

        public override ValueTask<TypeParserResult<IMapInstance>> ParseAsync(Parameter param, string value, CommandContext context)
        {
            if (!short.TryParse(value, out short mapId))
            {
                return new ValueTask<TypeParserResult<IMapInstance>>(new TypeParserResult<IMapInstance>($"The given map ID was invalid. ({value})"));
            }

            Guid mapGuid = _mapManager.GetBaseMapInstanceIdByMapId(mapId);
            IMapInstance map = _mapManager.GetMapInstance(mapGuid);

            return map is null
                ? new ValueTask<TypeParserResult<IMapInstance>>(new TypeParserResult<IMapInstance>($"A map with ID#{mapId} doesn't exist."))
                : new ValueTask<TypeParserResult<IMapInstance>>(new TypeParserResult<IMapInstance>(map));
        }
    }
}