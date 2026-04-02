using PiggyGame.Common.DTOs;
using PiggyGame.Common.DTOs.Users;

namespace PiggyGame.Services.Users;

public interface IUserService
{
    public Task<ScoreboardPagedList> GetScoreBoard(int limit, int offset);

    public Task<PigsRecordDto> GetGlobalPigsRecord();
    
    public Task<UserDescription> GetUserById(int userId);

    public Task UpdateUser(int userId, UpdateUserDto dto);
}