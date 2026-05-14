using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArlequimTest.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { email, role });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = _userService.Create(dto);
            return Created("", new { user.Id, user.Name, user.Email, user.Role });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode ,new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = _userService.Login(dto.Email, dto.Password);
            return Ok(new { token });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }
}
