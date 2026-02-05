using System;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    public class RegisterLoginResponse
    {
        public enum ResponseType
        {
            Success,
            Fail
        }

        public ResponseType Type { get; set; }
        public Guid Id { get; set; }
    }
}