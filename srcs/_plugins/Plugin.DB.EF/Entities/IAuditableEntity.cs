// NosEmu
// 


using System;

namespace Plugin.Database.Entities
{
    public interface IAuditableEntity
    {
        DateTime? DeletedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        DateTime? CreatedAt { get; set; }
    }
}