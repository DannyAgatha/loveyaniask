using System;
using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class CharacterLifetimeStatsDto
{
    /*
     * BATTLE
     */
    [ProtoMember(1)]
    public long TotalMonstersKilled { get; set; }

    [ProtoMember(2)]
    public long TotalPlayersKilled { get; set; }

    [ProtoMember(3)]
    public long TotalDeathsByMonster { get; set; }

    [ProtoMember(4)]
    public long TotalDeathsByPlayer { get; set; }

    [ProtoMember(5)]
    public long TotalSkillsCasted { get; set; }

    [ProtoMember(6)]
    public long TotalDamageDealt { get; set; }

    /*
     * FARMING
     */
    [ProtoMember(7)]
    public long TotalRaidsWon { get; set; }

    [ProtoMember(8)]
    public long TotalRaidsLost { get; set; }

    [ProtoMember(9)]
    public long TotalTimespacesWon { get; set; }

    [ProtoMember(10)]
    public long TotalTimespacesLost { get; set; }

    /*
     * EVENTS
     */
    [ProtoMember(11)]
    public long TotalInstantBattleWon { get; set; }

    [ProtoMember(12)]
    public long TotalIcebreakerWon { get; set; }

    /*
     * ECONOMY
     */
    [ProtoMember(13)]
    public long TotalGoldSpent { get; set; }

    [ProtoMember(14)]
    public long TotalGoldSpentInBazaarItems { get; set; }

    [ProtoMember(15)]
    public long TotalGoldSpentInBazaarFees { get; set; }

    [ProtoMember(16)]
    public long TotalGoldDropped { get; set; }

    [ProtoMember(17)]
    public long TotalGoldEarnedInBazaarItems { get; set; }

    [ProtoMember(18)]
    public long TotalGoldSpentInNpcShop { get; set; }

    /*
     * ITEMS
     */
    [ProtoMember(19)]
    public long TotalItemsUsed { get; set; }

    [ProtoMember(20)]
    public long TotalPotionsUsed { get; set; }

    [ProtoMember(21)]
    public long TotalSnacksUsed { get; set; }

    [ProtoMember(22)]
    public long TotalFoodUsed { get; set; }

    /*
     * OTHER
     */
    [ProtoMember(23)]
    public long TotalMinilandVisits { get; set; }

    [ProtoMember(24)]
    public TimeSpan TotalTimeOnline { get; set; }

    /*
     * ARENA
     */
    [ProtoMember(25)]
    public long TotalArenaDeaths { get; set; }

    [ProtoMember(26)]
    public long TotalArenaKills { get; set; }
}