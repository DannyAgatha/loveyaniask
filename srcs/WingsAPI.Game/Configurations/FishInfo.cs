using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WingsEmu.Game.Configurations
{
    [DataContract]
    public class FishInfo
    {
        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public double MinSize { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public double MaxSize { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public long ExpCollected { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public bool IsRareRewards { get; set; }
    }
}
