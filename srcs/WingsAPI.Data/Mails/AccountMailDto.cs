using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Mails;

public class AccountMailDto : ILongDto
{
    public long AccountId { get; set; }

    public DateTime Date { get; set; }

    public int ItemVnum { get; set; }

    public int Amount { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
}