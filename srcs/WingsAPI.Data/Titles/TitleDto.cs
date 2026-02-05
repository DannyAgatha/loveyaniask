// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Titles;

/// <summary>
///     Titles
/// </summary>
public class TitleDto : IDto
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     i18n key
    /// </summary>
    public string Name { get; set; }
}