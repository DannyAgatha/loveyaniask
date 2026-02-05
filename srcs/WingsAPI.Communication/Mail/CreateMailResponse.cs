using ProtoBuf;
using WingsEmu.DTOs.Mails;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class CreateMailResponse
    {
        [ProtoMember(1)]
        public RpcResponseType Status { get; set; }

        [ProtoMember(2)]
        public CharacterMailDto Mail { get; set; }
    }
}