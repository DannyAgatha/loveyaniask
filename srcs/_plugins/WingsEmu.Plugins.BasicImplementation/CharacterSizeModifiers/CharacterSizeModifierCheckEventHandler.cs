using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.CharacterSizeModifiers;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.CharacterSizeModifiers;

public class CharacterSizeModifierCheckEventHandler : IAsyncEventProcessor<CharacterSizeModifierCheckEvent>
{
    private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;

    public CharacterSizeModifierCheckEventHandler(CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
    {
        _characterSizeModifiersConfiguration = characterSizeModifiersConfiguration;
    }

    public async Task HandleAsync(CharacterSizeModifierCheckEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;

        int? hat = character.GetInventoryItemFromEquipmentSlot(EquipmentType.CostumeHat)?.ItemInstance?.ItemVNum;
        int? suit = character.GetInventoryItemFromEquipmentSlot(EquipmentType.CostumeSuit)?.ItemInstance?.ItemVNum;

        int defaultSize = _characterSizeModifiersConfiguration.SizeTypes.First(x => x.EntityType == EntityType.Player).DefaultSize;
        
        if (hat is null || suit is null)
        {
            character.ChangeSize(defaultSize);
            return;
        }

        CharacterSizeModifierSet matchingSet = _characterSizeModifiersConfiguration.Sets
            .FirstOrDefault(set => set.HatVnums.Contains(hat.Value) && set.CostumeVnums.Contains(suit.Value));

        int finalSize = matchingSet?.ReducedSize ?? defaultSize;
        character.ChangeSize(finalSize);
    }
}