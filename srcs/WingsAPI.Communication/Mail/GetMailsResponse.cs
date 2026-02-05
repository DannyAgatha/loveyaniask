using System.Collections.Generic;
using ProtoBuf;
using WingsEmu.DTOs.Mails;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class GetMailsResponse
    {
        [ProtoMember(1)]
        public IEnumerable<CharacterMailDto> CharacterMailsDto { get; set; }
    }
}