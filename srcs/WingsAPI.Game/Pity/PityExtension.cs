using System.Linq;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Pity;

public static class PityExtension
{
    public static bool IsPityUpgradeItem(this ItemInstanceDTO item, PityType pityType, PityConfiguration pityConfiguration)
    {
        PityInfo pityConfig = pityConfiguration.PityInfo.FirstOrDefault(s => s.PityType == pityType);
        
        if (pityConfig == null)
        {
            return false;
        }
        
        if (!item.PityCounter.ContainsKey((int)pityType))
        {
            item.PityCounter[(int)pityType] = 0;
        }
        
        if (pityType == PityType.Fairy)
        {
            return item.PityCounter[(int)pityType] == pityConfig.PityData.FirstOrDefault()?.MaxFailCount;
        }
        
        if (pityType is not (PityType.Equipment or PityType.Specialist))
        {
            if (!item.PityCounter.ContainsKey((int)pityType))
            {
                return false;
            }
            
            return item.PityCounter[(int)pityType] == pityConfig.PityData.FirstOrDefault()?.MaxFailCount;
        }
        
        int upgrade = item.Upgrade + 1;
        
        PityData pity = pityConfig.PityData.FirstOrDefault(s => s.Upgrade == upgrade);
        
        if (pity == null)
        {
            return false;
        }
        
        return pity.MaxFailCount == item.PityCounter[(int)pityType];
    }
    
    public static (int, int) ItemPityMaxFailCounter(this ItemInstanceDTO item, PityType pityType, PityConfiguration pityConfiguration)
    {
        int pityCount = item.PityCounter[(int)pityType];
        
        PityInfo pityConfig = pityConfiguration.PityInfo.FirstOrDefault(s => s.PityType == pityType);
        
        if (pityConfig == null)
        {
            return (pityCount, 0);
        }
        
        if (pityType is not (PityType.Equipment or PityType.Specialist))
        {
            return (pityCount, pityConfig.PityData.FirstOrDefault()?.MaxFailCount ?? 0);
        }
        
        int upgrade = item.Upgrade + 1;
        
        PityData pity = pityConfig.PityData.FirstOrDefault(s => s.Upgrade == upgrade);
        
        return pity == null ? (pityCount, 0) : (pityCount, pity.MaxFailCount);
    }
    
    private static int GetPityCounter(this IPlayerEntity player, PityType pityType)
    {
        CharacterPityDto pityEntry = player.PityDto.FirstOrDefault(p => p.PityType == pityType);
        if (pityEntry != null)
        {
            return pityEntry.PityCounter;
        }
        
        pityEntry = new CharacterPityDto { PityType = pityType, PityCounter = 0 };
        player.PityDto.Add(pityEntry);
        
        return pityEntry.PityCounter;
    }
    
    public static void IncrementPityCounter(this IPlayerEntity player, PityType pityType)
    {
        CharacterPityDto pityEntry = player.PityDto.FirstOrDefault(p => p.PityType == pityType);
        if (pityEntry == null)
        {
            pityEntry = new CharacterPityDto { PityType = pityType, PityCounter = 0 };
            player.PityDto.Add(pityEntry);
        }
        
        pityEntry.PityCounter++;
    }
    
    public static void ResetPityCounter(this IPlayerEntity player, PityType pityType)
    {
        CharacterPityDto pityEntry = player.PityDto.FirstOrDefault(p => p.PityType == pityType);
        if (pityEntry != null)
        {
            pityEntry.PityCounter = 0;
        }
    }
    
    public static bool IsPityUpgrade(this IPlayerEntity player, PityType pityType, PityConfiguration pityConfiguration)
    {
        PityInfo pityConfig = pityConfiguration.PityInfo.FirstOrDefault(s => s.PityType == pityType);
        
        if (pityConfig == null)
        {
            return false;
        }
        
        int currentPityCounter = player.GetPityCounter(pityType);
        
        return currentPityCounter >= pityConfig.PityData.FirstOrDefault()?.MaxFailCount;
    }
    
    public static (int, int) PityMaxFailCounter(this IPlayerEntity player, PityType pityType, PityConfiguration pityConfiguration)
    {
        int pityCount = player.GetPityCounter(pityType);
        
        PityInfo pityConfig = pityConfiguration.PityInfo.FirstOrDefault(s => s.PityType == pityType);
        
        return pityConfig == null ? (pityCount, 0) : (pityCount, pityConfig.PityData.FirstOrDefault()?.MaxFailCount ?? 0);
    }
    
    public static long GetGoldSpent(this IPlayerEntity player, PityType pityType)
    {
        CharacterPityDto pityEntry = player.PityDto.FirstOrDefault(p => p.PityType == pityType);
        return pityEntry?.GoldSpent ?? 0;
    }
    
    public static void AddGoldSpent(this IPlayerEntity player, PityType pityType, long gold)
    {
        CharacterPityDto pityEntry = player.PityDto.FirstOrDefault(p => p.PityType == pityType);
        if (pityEntry == null)
        {
            pityEntry = new CharacterPityDto
            {
                PityType = pityType,
                PityCounter = 0,
                GoldSpent = gold
            };
            player.PityDto.Add(pityEntry);
        }
        else
        {
            pityEntry.GoldSpent += gold;
        }
    }
    
    public static void ResetGoldSpent(this IPlayerEntity player, PityType pityType)
    {
        CharacterPityDto pityEntry = player.PityDto.FirstOrDefault(p => p.PityType == pityType);
        if (pityEntry != null)
        {
            pityEntry.GoldSpent = 0;
        }
    }
    
    public static bool IsPityBox(this GameItemInstance boxItem, PityConfiguration pityConfiguration, IPlayerEntity player)
    {
        PityInfo pityConfig = pityConfiguration.PityInfo.FirstOrDefault(s => s.PityType == PityType.RandomBox);

        if (pityConfig == null)
        {
            return false;
        }

        var pity = pityConfig.PityData.Where(s => s.ItemVnum == boxItem.ItemVNum).ToList();

        if (pity.Count == 0)
        {
            return false;
        }

        CharacterPityDto playerPity = player.PityDto.FirstOrDefault(p => p.ItemVnum == boxItem.ItemVNum);

        if (playerPity == null)
        {
            playerPity = new CharacterPityDto
            {
                ItemVnum = boxItem.ItemVNum,
                PityCounter = 0
            };
            player.PityDto.Add(playerPity);
        }

        if (boxItem.PityCounter.TryGetValue((int)PityType.RandomBox, out int value))
        {
            return pity.First().MaxFailCount == value;
        }

        value = playerPity.PityCounter;
        boxItem.PityCounter[(int)PityType.RandomBox] = value;

        return pity.First().MaxFailCount == value;
    }
    
    public static (int, int) BoxPityMaxFailCounter(this GameItemInstance boxItem, PityConfiguration pityConfiguration, IPlayerEntity player)
    {
        PityInfo pityConfig = pityConfiguration.PityInfo.FirstOrDefault(s => s.PityType == PityType.RandomBox);

        if (pityConfig == null)
        {
            return (0, 0);
        }

        var pity = pityConfig.PityData.Where(s => s.ItemVnum == boxItem.ItemVNum).ToList();

        CharacterPityDto playerPity = player.PityDto.FirstOrDefault(p => p.ItemVnum == boxItem.ItemVNum);

        if (playerPity == null)
        {
            playerPity = new CharacterPityDto
            {
                ItemVnum = boxItem.ItemVNum,
                PityCounter = 0
            };
            player.PityDto.Add(playerPity);
        }
        
        if (!boxItem.PityCounter.ContainsKey((int)PityType.RandomBox))
        {
            boxItem.PityCounter[(int)PityType.RandomBox] = playerPity.PityCounter;
        }

        int pityCount = playerPity.PityCounter;
        return pity.Any() ? (pityCount, pity.First().MaxFailCount) : (pityCount, 0);
    }
}