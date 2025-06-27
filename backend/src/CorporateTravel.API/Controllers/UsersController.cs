using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.Users.Queries.GetAllUsers;
using CorporateTravel.Application.Features.Users.Commands.UpdateUser;
using CorporateTravel.Application.Features.Users.Commands.DeleteUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CorporateTravel.API.Attributes;
using System.Threading.Tasks;

namespace CorporateTravel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles("Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserDto updateUserDto)
    {
        if (id != updateUserDto.Id)
        {
            return BadRequest("ID na URL não corresponde ao ID no corpo da requisição");
        }

        var command = new UpdateUserCommand
        {
            Id = updateUserDto.Id,
            Name = updateUserDto.Name,
            Email = updateUserDto.Email,
            Role = updateUserDto.Role
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var command = new DeleteUserCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }
} 