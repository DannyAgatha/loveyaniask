using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class ChangeFactionEventHandler : IAsyncEventProcessor<ChangeFactionEvent>
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IExpirableLockService _expirableLock;

    public ChangeFactionEventHandler(IGameLanguageService gameLanguage, ICharacterAlgorithm characterAlgorithm, IExpirableLockService expirableLock)
    {
        _gameLanguage = gameLanguage;
        _characterAlgorithm = characterAlgorithm;
        _expirableLock = expirableLock;
    }

    public async Task HandleAsync(ChangeFactionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        
        session.PlayerEntity.SetFaction(e.NewFaction);
        
        GameDialogKey factionMessageKey = e.NewFaction == FactionType.Neutral
            ? GameDialogKey.INTERACTION_SHOUTMESSAGE_GET_PROTECTION_POWER_NEUTRAL
            : e.NewFaction == FactionType.Angel
                ? GameDialogKey.INTERACTION_SHOUTMESSAGE_GET_PROTECTION_POWER_ANGEL
                : GameDialogKey.INTERACTION_SHOUTMESSAGE_GET_PROTECTION_POWER_DEMON;
                
        session.SendMsg(_gameLanguage.GetLanguage(factionMessageKey, session.UserLanguage), MsgMessageType.Middle);
        session.SendPacket("scr 0 0 0 0 0 0");
        session.RefreshFaction();
        session.RefreshStatChar();
        session.SendEffect(e.NewFaction == FactionType.Demon ? EffectType.DemonProtection : EffectType.AngelProtection);
        session.SendCondPacket();
        session.RefreshLevel(_characterAlgorithm);
    }
}
