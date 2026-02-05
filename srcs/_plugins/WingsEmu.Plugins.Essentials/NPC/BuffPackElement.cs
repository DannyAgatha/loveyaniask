using System;
using YamlDotNet.Serialization;

namespace WingsEmu.Plugins.Essentials.NPC;

public class BuffPackElement
{
    [YamlMember(Alias = "card_id")]
    public int CardId { get; set; }

    [YamlMember(Alias = "level")]
    public int Level { get; set; }

    [YamlMember(Alias = "duration")]
    public TimeSpan Duration { get; set; }

    [YamlMember(Alias = "keep_on_death")]
    public bool KeepOnDeath { get; set; }
}