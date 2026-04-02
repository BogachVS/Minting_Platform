using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiggyGame.Common.DTOs;
using PiggyGame.Common.DTOs.Users;
using PiggyGame.Services.Users;

namespace PiggyGame.Controllers;

[Authorize]
public class UserController : ApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("scoreboard")]
    [AllowAnonymous]
    public async Task<ScoreboardPagedList> GetUsersScoreboard([FromQuery] PagedListRequest request)
    {
        return await _userService.GetScoreBoard(request.Limit, request.Offset);
    }
    
    [HttpGet("pigs-record")]
    public async Task<PigsRecordDto> GetGlobalPigsRecord()
    {
        return await _userService.GetGlobalPigsRecord();
    }

    [HttpGet]
    public async Task<UserDescription> GetUser()
    {
        var userId = GetUserId();
        return await _userService.GetUserById(userId);
    }
    
    [HttpPut]
    public async Task UpdateUser([FromBody] UpdateUserDto dto)
    {
        var userId = GetUserId();
        await _userService.UpdateUser(userId, dto);
    }
}