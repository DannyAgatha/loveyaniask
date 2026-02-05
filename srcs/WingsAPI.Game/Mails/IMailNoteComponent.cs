using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Generics;

namespace WingsEmu.Game.Mails;

public interface IMailNoteComponent
{
    void AddMail(CharacterMail mailDto);
    void RemoveMail(CharacterMail mailDto);
    CharacterMail GetMail(long mailId);
    IEnumerable<CharacterMail> GetMails();

    void AddNote(CharacterNote noteDto);
    void RemoveNote(CharacterNote noteDto);
    CharacterNote GetNote(long noteId, bool isSenderCopy);
    IEnumerable<CharacterNote> GetNotes();
}

public class MailNoteComponent : IMailNoteComponent
{
    private readonly ThreadSafeList<CharacterMail> _mailLists = new();
    private readonly ConcurrentDictionary<long, CharacterMail> _mails = new();
    private readonly ThreadSafeList<CharacterNote> _noteLists = new();
    private readonly ConcurrentDictionary<long, (CharacterNote, bool)> _notes = new();

    public void AddMail(CharacterMail mailDto)
    {
        _mails.TryAdd(mailDto.MailSlot, mailDto);
        _mailLists.Add(mailDto);
    }

    public void RemoveMail(CharacterMail mailDto)
    {
        _mails.TryRemove(mailDto.MailSlot, out _);
        _mailLists.Remove(mailDto);
    }

    public CharacterMail GetMail(long mailId) => _mails.TryGetValue(mailId, out CharacterMail mail) ? mail : null;

    public IEnumerable<CharacterMail> GetMails() => _mailLists;

    public void AddNote(CharacterNote noteDto)
    {
        _notes.TryAdd(noteDto.NoteSlot, (noteDto, noteDto.IsSenderCopy));
        _noteLists.Add(noteDto);
    }

    public void RemoveNote(CharacterNote noteDto)
    {
        _notes.TryRemove(noteDto.NoteSlot, out _);
        _noteLists.Remove(noteDto);
    }

    public CharacterNote GetNote(long noteId, bool isSenderCopy)
    {
        CharacterNote note = _noteLists.FirstOrDefault(x => x.NoteSlot == noteId && isSenderCopy == x.IsSenderCopy);
        return note;
    }

    public IEnumerable<CharacterNote> GetNotes() => _noteLists;
}