using System.Collections.Generic;
using WingsEmu.DTOs.Quicklist;

namespace WingsEmu.Game.Quicklist;

public interface IQuicklistComponent
{
    List<CharacterQuicklistEntryDto> GetQuicklist();
    IReadOnlyList<CharacterQuicklistEntryDto> GetQuicklistByTab(short tab, int morphId);
    CharacterQuicklistEntryDto GetQuicklistByTabSlotAndMorph(short tab, short slot, int morphId);

    void AddQuicklist(CharacterQuicklistEntryDto quicklist);
    void RemoveQuicklist(short tab, short slot, int morphId);
}