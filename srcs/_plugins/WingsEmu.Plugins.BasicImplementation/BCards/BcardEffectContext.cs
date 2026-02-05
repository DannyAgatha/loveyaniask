using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.ServerPackets.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards;

public class BcardEffectContext : IBCardEffectContext
{
    public BcardEffectContext(IBattleEntity sender, IBattleEntity target, BCardDTO bCard, SkillInfo skill = null, Position position = default, 
        SuPacketHitMode hitMode = SuPacketHitMode.SuccessAttack, int damageDealt = 0)
    {
        Sender = sender;
        Target = target;
        BCard = bCard;
        Skill = skill;
        Position = position;
        HitMode = hitMode;
        DamageDealt = damageDealt;
    }

    public IBattleEntity Sender { get; }
    public IBattleEntity Target { get; }
    public BCardDTO BCard { get; }
    public SkillInfo Skill { get; }
    public Position Position { get; }
    public SuPacketHitMode HitMode { get; }
    public int DamageDealt { get; }
}