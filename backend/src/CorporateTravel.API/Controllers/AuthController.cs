using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.Authentication.Commands.LoginUser;
using CorporateTravel.Application.Features.Authentication.Commands.RegisterUser;
using CorporateTravel.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CorporateTravel.API.Attributes;
using System.Threading.Tasks;

namespace CorporateTravel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPasswordRequirementsService _passwordRequirementsService;

    public AuthController(IMediator mediator, IPasswordRequirementsService passwordRequirementsService)
    {
        _mediator = mediator;
        _passwordRequirementsService = passwordRequirementsService;
    }

    [HttpGet("password-requirements")]
    public IActionResult GetPasswordRequirements()
    {
        var requirements = _passwordRequirementsService.GetPasswordRequirements();
        return Ok(requirements);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        var command = new RegisterUserCommand
        {
            Name = registerUserDto.Name,
            Email = registerUserDto.Email,
            Password = registerUserDto.Password,
            Role = registerUserDto.Role
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return StatusCode(201);
    }

    [HttpPost("register-admin")]
    [AuthorizeRoles("Admin")]
    public async Task<IActionResult> RegisterByAdmin(RegisterUserDto registerUserDto)
    {
        var command = new RegisterUserCommand
        {
            Name = registerUserDto.Name,
            Email = registerUserDto.Email,
            Password = registerUserDto.Password,
            Role = registerUserDto.Role
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto loginUserDto)
    {
        var command = new LoginUserCommand
        {
            Email = loginUserDto.Email,
            Password = loginUserDto.Password
        };

        var result = await _mediator.Send(command);

        if (result == null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }
} 