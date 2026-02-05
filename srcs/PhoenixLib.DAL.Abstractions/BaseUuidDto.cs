using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoenixLib.DAL
{
    public class BaseUuidDto : IUuidDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}