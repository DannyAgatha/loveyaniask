using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums;

namespace WingsEmu.Game.Configurations;

public class GeneralEvtbFile
{
    public List<AutomaticEvtbFile> AutomaticEvents { get; set; }
}

public class AutomaticEvtbFile
{
    public DateTime StartDateTime { get; set; }
    public List<ConfigurationEvtb> Events { get; set; }
    public DateTime EndDateTime { get; set; }
}

public class ConfigurationEvtb
{
    public EvtbType EventType { get; set; }
    public int Value { get; set; }
}