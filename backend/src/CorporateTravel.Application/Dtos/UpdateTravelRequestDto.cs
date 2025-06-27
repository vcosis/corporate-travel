using System;
using System.ComponentModel.DataAnnotations;

namespace CorporateTravel.Application.Dtos;

public class UpdateTravelRequestDto
{
    [Required]
    public string Origin { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public string Reason { get; set; } = string.Empty;
} 