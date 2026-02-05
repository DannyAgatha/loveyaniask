// NosEmu
// 


using WingsEmu.Game.Battle;

namespace WingsEmu.Game._ECS.Systems;

public interface IBattleSystem
{
    void AddCastHitRequest(HitProcessable hitProcessable);
    void AddCastBuffRequest(BuffProcessable buffProcessable);
    void AddHitRequest(HitRequest hitRequest);
    void AddBuffRequest(BuffRequest buffRequest);
}