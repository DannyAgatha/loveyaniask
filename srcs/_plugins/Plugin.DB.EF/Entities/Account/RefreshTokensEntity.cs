using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;

namespace Plugin.Database.Entities.Account;

[Table("refresh_tokens", Schema = DatabaseSchemas.ACCOUNTS)]
public class RefreshTokensEntity : ILongEntity
{
    public string Token { get; set; }
    
    public DateTime Expires { get; set; }
    
    public bool IsValid { get; set; }
    
    public virtual AccountEntity AccountEntity { get; set; }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
}