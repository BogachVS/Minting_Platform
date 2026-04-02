using MediatR;
using Microsoft.AspNetCore.Mvc;
using PiggyGame.Common.Constants.Auth;

namespace PiggyGame.Controllers;

/// <summary>
/// Base controller which adds the <c>Api</c> prefix to all inheritance controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    protected int GetUserId()
    {
        var subClaim = User.Claims.FirstOrDefault(x => x.Type == JwtClaims.InternalId);
        if (subClaim == null)
        {
            throw new Exception("Not valid JWT: name identifier not provide");
        }
        
        var result = int.TryParse(subClaim.Value, out var id);
        if (!result)
        {
            throw new Exception("Not valid JWT: incorrect name identifier format");
        }
    
        return id;
    }  
    
    protected long GetTelegramId()
    {
        var subClaim = User.Claims.FirstOrDefault(x => x.Type == JwtClaims.Sub);
        if (subClaim == null)
        {
            throw new Exception("Not valid JWT: name identifier not provide");
        }
        
        var result = int.TryParse(subClaim.Value, out var id);
        if (!result)
        {
            throw new Exception("Not valid JWT: incorrect name identifier format");
        }
    
        return id;
    }  
}
