using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations
{
    [DataContract]
    public class FairyConfig
    {
        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public byte FairyType { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public List<int> AllowedFairyVnum { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public long GoldPrice { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public long SucessRate { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = false)]
        public long FairyVnumCreated { get; set; }

        [DataMember(EmitDefaultValue = true, IsRequired = true)]
        public List<SpecialItem> SpecialItemsNeeded { get; set; } = new()
        {
            new()
        };
    }
    [DataContract]
    public class CreateFairyAct6Configuration : List<FairyConfig>
    {

    }
}