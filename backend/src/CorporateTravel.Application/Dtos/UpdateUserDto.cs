using System.ComponentModel.DataAnnotations;

namespace CorporateTravel.Application.Dtos;

public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
} 