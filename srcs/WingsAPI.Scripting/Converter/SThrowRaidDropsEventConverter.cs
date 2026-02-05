using System;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using WingsAPI.Scripting.Event.Raid;
using WingsEmu.Core;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Raids.Events;

namespace WingsAPI.Scripting.Converter
{
    public class SThrowRaidDropsEventConverter : ScriptedEventConverter<SThrowRaidDropsEvent>
    {
        private readonly Dictionary<Guid, IMonsterEntity> monsters;

        public SThrowRaidDropsEventConverter(Dictionary<Guid, IMonsterEntity> monsters) => this.monsters = monsters;

        protected override IAsyncEvent Convert(SThrowRaidDropsEvent e)
        {
            return new RaidMonsterThrowEvent(monsters[e.BossId], e.Drops.Select(x => new Drop
            {
                ItemVNum = x.ItemVnum,
                Amount = x.Amount
            }).ToList(), e.DropsStackCount, new Range<int>
            {
                Minimum = e.GoldRange.Minimum,
                Maximum = e.GoldRange.Maximum
            }, e.GoldStackCount);
        }
    }
}