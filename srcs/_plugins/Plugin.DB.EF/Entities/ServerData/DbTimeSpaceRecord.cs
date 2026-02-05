using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Plugin.Database.DB;

namespace Plugin.Database.Entities.ServerData
{
    [Table("time_space_records", Schema = DatabaseSchemas.CHARACTERS)]
    public class DbTimeSpaceRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TimeSpaceId { get; set; }

        public string CharacterName { get; set; }

        public long Record { get; set; }

        public DateTime Date { get; set; }
    }
}