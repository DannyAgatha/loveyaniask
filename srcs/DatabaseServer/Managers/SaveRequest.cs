using System;

namespace DatabaseServer.Managers
{
    internal record SaveRequest
    {
        public DateTime CreatedAt { get; init; }
        public long CharacterId { get; init; }
    }
}