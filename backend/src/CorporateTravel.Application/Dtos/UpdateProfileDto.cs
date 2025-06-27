using System.ComponentModel.DataAnnotations;

namespace CorporateTravel.Application.Dtos;

public class UpdateProfileDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6)]
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? NewPassword { get; set; }
} 