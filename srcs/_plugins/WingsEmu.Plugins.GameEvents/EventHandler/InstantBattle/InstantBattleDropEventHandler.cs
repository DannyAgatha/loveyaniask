using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.Event.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.InstantBattle
{
    public class InstantBattleDropEventHandler : IAsyncEventProcessor<InstantBattleDropEvent>
    {

        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;

        public InstantBattleDropEventHandler(IGameItemInstanceFactory gameItemInstanceFactory)
        {
            _gameItemInstanceFactory = gameItemInstanceFactory;
        }

        public async Task HandleAsync(InstantBattleDropEvent e, CancellationToken cancellation)
        {
            IMapInstance map = e.Instance.MapInstance;

            if (map.GetAliveMonsters(x => x.IsInstantBattle).Count > 0 || e.Wave.Drops == null || e.Wave.Drops.Count < 1)
            {
                return;
            }
            
            foreach (IClientSession mapInstanceSession in map.Sessions)
            {
                float requiredDamageModifier = mapInstanceSession.PlayerEntity.Level < 20 ? 100 
                    : mapInstanceSession.PlayerEntity.Level < 50 ? 500 
                    : mapInstanceSession.PlayerEntity.Level < 80 ? 1000 
                    : mapInstanceSession.PlayerEntity.HeroLevel > 0 ? 4000 : 2000;
                
                long damage = mapInstanceSession.PlayerEntity.InstantCombatDamage;
                mapInstanceSession.PlayerEntity.InstantCombatDamage = 0;
                
                if ((mapInstanceSession.PlayerEntity.HeroLevel > 0 ? mapInstanceSession.PlayerEntity.HeroLevel : mapInstanceSession.PlayerEntity.Level) * requiredDamageModifier > damage)
                {
                    mapInstanceSession.SendSayi(ChatMessageColorType.Red, Game18NConstString.NotEnoughActionPoints);
                    continue;
                }
                
                foreach (InstantBattleDrop drop in e.Wave.Drops)
                {
                    
                    if (drop.ItemVnum == (short)ItemVnums.GOLD)
                    {
                        await mapInstanceSession.EmitEventAsync(new GenerateGoldEvent(drop.AmountPerBunch, fallBackToBank: true, sendMessage: false));
                        mapInstanceSession.SendSayi(ChatMessageColorType.Green, Game18NConstString.VictoryReceivedGold, 4, drop.AmountPerBunch);
                        continue;
                    }

                    GameItemInstance item = _gameItemInstanceFactory.CreateItem(drop.ItemVnum, drop.AmountPerBunch);
                    await mapInstanceSession.AddNewItemToInventory(item, showMessage: true, sendGiftIsFull: true);
                }
                
                mapInstanceSession.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.LevelRewardInInventory, 4, e.Wave.WaveLevel);
            }
        }
    }
}