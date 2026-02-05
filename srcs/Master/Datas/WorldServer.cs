// NosEmu
// 


using System;
using Master.Proxies;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.DTOs.Account;

namespace Master.Datas
{
    public class WorldServer
    {
        public int ChannelId { get; init; }
        public GameChannelType ChannelType { get; init; }
        public int AccountLimit { get; set; }
        public string WorldGroup { get; set; }
        public AuthorityType AuthorityRequired { get; set; } = AuthorityType.User;
        public DateTime LastPulse { get; set; }
        public int SessionsCount { get; set; }
        public string EndPointIp { get; set; }
        public int EndPointPort { get; set; }
        public DateTime RegistrationDate { get; init; }
    }
}