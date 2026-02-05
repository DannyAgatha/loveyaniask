using System;
using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerSaveCharactersRequest
    {
        [ProtoMember(1)]
        public List<CharacterDTO> Characters { get; set; }

        [ProtoMember(2)]
        public DateTime Date { get; set; }
    }
}