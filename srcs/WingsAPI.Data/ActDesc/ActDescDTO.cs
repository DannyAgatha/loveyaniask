using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WingsAPI.Data.ActDesc;

public class ActDescDTO
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte Act { get; set; }

    public byte SubAct { get; set; }
    public byte TsAmount { get; set; }
    public string ActName { get; set; }
}