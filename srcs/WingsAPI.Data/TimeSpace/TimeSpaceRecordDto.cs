using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProtoBuf;

namespace WingsAPI.Data.TimeSpace;

[ProtoContract]
public class TimeSpaceRecordDto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [ProtoMember(1)]
    public long TimeSpaceId { get; set; }

    [ProtoMember(2)]
    public string CharacterName { get; set; }

    [ProtoMember(3)]
    public long Record { get; set; }

    [ProtoMember(4)]
    public DateTime Date { get; set; }
}