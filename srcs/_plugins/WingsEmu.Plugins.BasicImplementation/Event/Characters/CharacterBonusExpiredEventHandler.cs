using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class CharacterBonusExpiredEventHandler : IAsyncEventProcessor<CharacterBonusExpiredEvent>
{
    public async Task HandleAsync(CharacterBonusExpiredEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        DateTime currentTime = DateTime.UtcNow;

        Queue<CharacterStaticBonusDto> toRemove = new();
        foreach (CharacterStaticBonusDto staticBonus in session.PlayerEntity.Bonus.ToArray())
        {
            if (staticBonus.DateEnd == null)
            {
                continue;
            }

            if (staticBonus.DateEnd.Value >= currentTime)
            {
                continue;
            }

            toRemove.Enqueue(staticBonus);
        }

        while (toRemove.TryDequeue(out CharacterStaticBonusDto bonus))
        {
            session.PlayerEntity.Bonus.Remove(bonus);

            switch (bonus.StaticBonusType)
            {
                case StaticBonusType.AdventurerMedal:
                case StaticBonusType.EreniaMedal:
                case StaticBonusType.InventoryExpansion:
                case StaticBonusType.Backpack:
                    session.ShowInventoryExtensions();
                    break;
                case StaticBonusType.PetBasket:
                    session.SendPetBasketPacket(false);
                    break;
            }
        }

        session.SendStaticBonuses();
    }
}