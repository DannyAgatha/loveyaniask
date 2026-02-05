using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plugin.Database.Entities
{
    public abstract class BaseAuditableEntity : IAuditableEntity
    {
        [Column(Order = 50)]
        public DateTime? CreatedAt { get; set; }

        [Column(Order = 51)]
        public DateTime? UpdatedAt { get; set; }

        [Column(Order = 52)]
        public DateTime? DeletedAt { get; set; }
    }
}