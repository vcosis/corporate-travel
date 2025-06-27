using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.Profile.Queries.GetProfile;
using CorporateTravel.Application.Features.Profile.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CorporateTravel.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuário não identificado");
        }

        var query = new GetProfileQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateProfileDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Usuário não identificado");
        }

        var command = new UpdateProfileCommand
        {
            UserId = userId,
            Name = updateProfileDto.Name,
            CurrentPassword = updateProfileDto.CurrentPassword,
            NewPassword = updateProfileDto.NewPassword
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Perfil atualizado com sucesso" });
    }
} 