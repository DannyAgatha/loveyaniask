using System;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class FactionSwitchGuriHandler : IGuriHandler
{
    private const int IndividualAngelEgg = 1;
    private const int IndividualDemonEgg = 2;
    private const int FamilyAngelEgg = 3;
    private const int FamilyDemonEgg = 4;
    
    private readonly IExpirableLockService _expirableLockService;

    public FactionSwitchGuriHandler(IExpirableLockService expirableLockService)
    {
        _expirableLockService = expirableLockService;
    }
    public long GuriEffectId => 750;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        int eggType = e.Data;
        int vnum = 1623 + eggType;
        var targetFaction = (FactionType)eggType;

        bool hasItem = session.PlayerEntity.HasItem(vnum);

        if (!hasItem)
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        switch (eggType)
        {
            case IndividualAngelEgg:
            case IndividualDemonEgg:
            {
                if (session.PlayerEntity.IsInFamily())
                {
                    return;
                }

                if (session.PlayerEntity.Faction == targetFaction)
                {
                    return;
                }
                
                bool isPlayerUnderCooldown = await _expirableLockService.ExistsTemporaryLock($"game:locks:character-change-faction:{session.PlayerEntity.Id}");
                if (isPlayerUnderCooldown)
                {
                    DateTime? lockExpiration = await _expirableLockService.GetLockExpirationAsync($"game:locks:character-change-faction:{session.PlayerEntity.Id}");
                    
                    if (!lockExpiration.HasValue)
                    {
                        return;
                    }

                    TimeSpan remainingTime = lockExpiration.Value - DateTime.UtcNow;
                    string formattedTime = remainingTime.ToReadableString();
                    session.SendInfo(session.GetLanguageFormat(GameDialogKey.CANT_CHANGE_FACTION_CHARACTER_DELAY, formattedTime));
                    return;
                }
                
                await session.EmitEventAsync(new ChangeFactionEvent
                {
                    NewFaction = targetFaction
                });

                await session.RemoveItemFromInventory(vnum);
                break;
            }
            case FamilyAngelEgg:
            case FamilyDemonEgg:
            {
                string lockKey = $"game:locks:family:{session.PlayerEntity.Family.Id}:change-faction";
                DateTime? lockExpiration = await _expirableLockService.GetLockExpirationAsync(lockKey);
    
                if (lockExpiration.HasValue && lockExpiration.Value > DateTime.UtcNow)
                {
                    TimeSpan remainingTime = lockExpiration.Value - DateTime.UtcNow;
                    string formattedTime = remainingTime.ToReadableString();
                    session.SendInfo(session.GetLanguageFormat(GameDialogKey.CANT_CHANGE_FACTION_FAMILY_DELAY, formattedTime));
                    return;
                }
                
                await session.EmitEventAsync(new FamilyChangeFactionEvent
                {
                    Faction = eggType / 2
                });
                break;
            }
        }
    }
}