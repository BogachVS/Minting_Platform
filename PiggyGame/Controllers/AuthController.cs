using Microsoft.AspNetCore.Mvc;
using PiggyGame.Common.DTOs.Auth;
using PiggyGame.Services.Auth;

namespace PiggyGame.Controllers;

public class AuthController : ApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<AccessToken> Authorize([FromBody] AuthRequest request)
    {
        return await _authService.Authorize(request.InitData, request.ReferralCode);
    }

    [HttpPost("code")]
    public async Task<AccessToken> AuthorizeWithCode([FromBody] AuthWithCodeRequest request)
    {
        return await _authService.AuthorizeWithCode(request.Code);
    }
}
