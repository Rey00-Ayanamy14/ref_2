using CourierManagementSystem.Api.Constants;
using CourierManagementSystem.Api.Models.DTOs.Requests;
using CourierManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CourierManagementSystem.Api.Controllers;

[ApiController]
[Route(ApiConstants.AuthRoutePrefix)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost(ApiConstants.LoginEndpoint)]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpGet(ApiConstants.CurrentUserEndpoint)]
    [AllowAnonymous]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var login = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            login,
            role,
            isAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }
}
