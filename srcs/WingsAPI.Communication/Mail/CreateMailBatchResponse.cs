using System.Collections.Generic;
using ProtoBuf;
using WingsEmu.DTOs.Mails;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class CreateMailBatchResponse
    {
        [ProtoMember(1)]
        public RpcResponseType Status { get; set; }

        [ProtoMember(2)]
        public IEnumerable<CharacterMailDto> Mail { get; set; }
    }
}