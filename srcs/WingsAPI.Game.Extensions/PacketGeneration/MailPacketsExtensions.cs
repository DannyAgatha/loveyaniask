using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class MailPacketsExtensions
    {
        /* Generate Packets */

        public static string GeneratePost(this IClientSession session, CharacterNote characterNote, byte type) =>
            $"post 1 {type} {characterNote.NoteSlot} 0 {(characterNote.IsOpened ? 1 : 0)} {characterNote.Date:yyMMddHHmm} " +
            $"{(type == 2 ? characterNote.ReceiverName : characterNote.SenderName)} {characterNote.Title}";

        public static string GeneratePostMessage(this IClientSession session, CharacterNote characterNote, byte type) =>
            $"post 5 {type} {characterNote.NoteSlot} 0 0 {(byte)characterNote.SenderClass} " +
            $"{(byte)characterNote.SenderGender} -1 {(byte)characterNote.SenderHairStyle} {(byte)characterNote.SenderHairColor} {characterNote.EquipmentPackets} " +
            $"{characterNote.SenderName} {characterNote.Title.Replace(' ', (char)0xB)} {characterNote.Message.Replace(' ', (char)0xB)}";

        public static string GenerateParcel(this IClientSession session, CharacterMail characterMail) =>
            $"parcel 1 1 {characterMail.MailSlot} {(byte)characterMail.MailGiftType} 0 " +
            $"{characterMail.Date:yyMMddHHmm} {characterMail.SenderName} {characterMail.ItemInstance.ItemVNum} " +
            $"{characterMail.ItemInstance.Amount} {(byte)characterMail.ItemInstance.GameItem.Type}";

        public static string GenerateParcelDelete(this IClientSession session, byte type, long mailId) => $"parcel {type} 1 {mailId}";

        public static string GenerateNoteDelete(this IClientSession session, long noteId, bool isSenderCopy) => $"post 2 {(isSenderCopy ? 2 : 1)} {noteId}";

        /* Send Packets */

        public static void SendPost(this IClientSession session, CharacterNote characterNote, byte type) =>
            session.SendPacket(session.GeneratePost(characterNote, type));

        public static void SendPostMessage(this IClientSession session, CharacterNote characterNote, byte type) =>
            session.SendPacket(session.GeneratePostMessage(characterNote, type));

        public static void SendParcel(this IClientSession session, CharacterMail characterMail) => session.SendPacket(session.GenerateParcel(characterMail));

        public static void SendParcelDelete(this IClientSession session, byte type, long mailId) => session.SendPacket(session.GenerateParcelDelete(type, mailId));

        public static void SendNoteDelete(this IClientSession session, long noteId, bool isSenderCopy) => session.SendPacket(session.GenerateNoteDelete(noteId, isSenderCopy));

        public static void SendMailPacket(this IClientSession session, CharacterNote characterNote)
        {
            if (!characterNote.IsSenderCopy && characterNote.ReceiverId == session.PlayerEntity.Id)
            {
                session.SendPost(characterNote, 1);
                return;
            }

            session.SendPost(characterNote, 2);
        }

        public static byte GetNextMailSlot(this IClientSession session)
        {
            byte slot;
            IEnumerable<CharacterMail> mails = session.PlayerEntity.MailNoteComponent.GetMails().ToArray();
            for (slot = 0; slot < mails.Count(); slot++)
            {
                CharacterMail mail = session.PlayerEntity.MailNoteComponent.GetMail(slot);
                if (mail != null)
                {
                    continue;
                }

                break;
            }

            return slot;
        }

        public static byte GetNextNoteSlot(this IClientSession session, bool isSenderCopy)
        {
            byte slot;
            CharacterNote[] notesCopy = session.PlayerEntity.MailNoteComponent.GetNotes().Where(x => x.IsSenderCopy).ToArray();
            for (slot = 0; slot < notesCopy.Length; slot++)
            {
                CharacterNote mail = session.PlayerEntity.MailNoteComponent.GetNote(slot, isSenderCopy);
                if (mail != null)
                {
                    continue;
                }

                break;
            }

            return slot;
        }
    }
}