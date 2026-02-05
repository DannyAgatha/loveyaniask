// NosEmu
// 


using System;

namespace PhoenixLib.DAL
{
    public interface IUuidDto : IDto
    {
        Guid Id { get; set; }
    }
}