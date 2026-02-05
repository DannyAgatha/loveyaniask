using System.Collections.Generic;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Quicklist;

namespace WingsEmu.Game.Quicklist;

public class QuicklistComponent : IQuicklistComponent
{
    private const int QUICKLIST_SLOTS = 30;
    private readonly List<CharacterQuicklistEntryDto> _quicklist = new();
    private readonly Dictionary<(short, int), CharacterQuicklistEntryDto[]> _quicklistByTabAndMorphId = new();


    public List<CharacterQuicklistEntryDto> GetQuicklist() => _quicklist;

    public IReadOnlyList<CharacterQuicklistEntryDto> GetQuicklistByTab(short tab, int morphId) => _quicklistByTabAndMorphId.GetOrDefault((tab, morphId));

    public CharacterQuicklistEntryDto GetQuicklistByTabSlotAndMorph(short tab, short slot, int morphId)
    {
        CharacterQuicklistEntryDto[] entries = _quicklistByTabAndMorphId.GetOrDefault((tab, morphId));
        return entries?[slot];
    }

    public void AddQuicklist(CharacterQuicklistEntryDto quicklist)
    {
        if (!_quicklistByTabAndMorphId.TryGetValue((quicklist.QuicklistTab, quicklist.Morph), out CharacterQuicklistEntryDto[] dtos))
        {
            _quicklistByTabAndMorphId[(quicklist.QuicklistTab, quicklist.Morph)] = dtos = new CharacterQuicklistEntryDto[QUICKLIST_SLOTS];
        }

        dtos[quicklist.QuicklistSlot] = quicklist;
        _quicklist.Add(quicklist);
    }

    public void RemoveQuicklist(short tab, short slot, int morphId)
    {
        CharacterQuicklistEntryDto[] quicklist = _quicklistByTabAndMorphId.GetOrDefault((tab, morphId));
        quicklist[slot] = null;
        _quicklist.RemoveAll(s => s.QuicklistTab == tab && s.QuicklistSlot == slot && s.Morph == morphId);
    }
}