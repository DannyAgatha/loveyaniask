using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;

namespace WingsEmu.Commands.TypeParsers
{
    public sealed class ItemTypeParser : TypeParser<IGameItem>
    {
        private readonly IItemsManager _itemManager;

        public ItemTypeParser(IItemsManager itemsManager) => _itemManager = itemsManager;

        public override ValueTask<TypeParserResult<IGameItem>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            if (!short.TryParse(value, out short itemVNum))
            {
                return new ValueTask<TypeParserResult<IGameItem>>(new TypeParserResult<IGameItem>($"The given Item ID was invalid. ({value})"));
            }

            IGameItem gameItem = _itemManager.GetItem(itemVNum);

            return gameItem is null
                ? new ValueTask<TypeParserResult<IGameItem>>(new TypeParserResult<IGameItem>($"There is no Item with ID#{itemVNum}"))
                : new ValueTask<TypeParserResult<IGameItem>>(new TypeParserResult<IGameItem>(gameItem));
        }
    }
}