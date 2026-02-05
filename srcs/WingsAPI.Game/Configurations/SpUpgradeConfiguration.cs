using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class SpUpgradeConfiguration : List<UpgradeConfiguration>
{
}