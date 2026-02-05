using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Act6;

public class TeleportCylloanHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IBuffFactory _buffFactory;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    public TeleportCylloanHandler(IMapManager mapManager, IGameLanguageService gameLanguage, IBuffFactory buffFactory, ICharacterAlgorithm characterAlgorithm)
    {
        _gameLanguage = gameLanguage;
        _buffFactory = buffFactory;
        _characterAlgorithm = characterAlgorithm;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ACT61_TELEPORT_CYLLOAN };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.Level < 86)
        {
            session.SendInformationChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_REQUIERED_LEVEL, session.UserLanguage));
            return;
        }

        if (npcEntity.Id != 1250)
        {
            return;
        }
        
        session.ChangeMap(228, 68, 103);

        if (session.PlayerEntity.HeroLevel != 0)
        {
            return;
        }
        
        session.PlayerEntity.HeroLevel++;
        session.PlayerEntity.HeroXp = 0;
        await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_HERO, session.PlayerEntity));
        session.PlayerEntity.Session.RefreshStat();
        session.RefreshStat();
        session.RefreshLevel(_characterAlgorithm);
    }
}