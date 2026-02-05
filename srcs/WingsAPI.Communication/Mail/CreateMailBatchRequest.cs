using System.Collections.Generic;
using ProtoBuf;
using WingsEmu.DTOs.Mails;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class CreateMailBatchRequest
    {
        [ProtoMember(1)]
        public List<CharacterMailDto> Mails { get; set; }

        [ProtoMember(2)]
        public bool Bufferized { get; set; }
    }
}