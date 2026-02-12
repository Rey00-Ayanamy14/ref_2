using CourierManagementSystem.Api.Constants;
using CourierManagementSystem.Api.Models.DTOs.Requests;
using CourierManagementSystem.Api.Models.DTOs.Responses;
using CourierManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourierManagementSystem.Api.Controllers;

[ApiController]
[Route(ApiConstants.AuthRoutePrefix)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    [HttpPost(ApiConstants.LoginEndpoint)]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null");
        }

        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpGet(ApiConstants.CurrentUserEndpoint)]
    [AllowAnonymous] 
    public IActionResult GetCurrentUser()
    {
        var userInfo = _currentUserService.GetCurrentUserInfo();
        return Ok(userInfo);
    }
}
