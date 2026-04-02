using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace PiggyGame.Common.WebExtensions;

public static class JwtAuthenticationExtension
{
    public static void AddJwtAuthentication(this IServiceCollection serviceCollection, ConfigurationManager config)
    {
        var accessTokenSecret = config["AuthTokens:AccessTokenSecret"];
        if (accessTokenSecret == null)
        {
            throw new ApplicationException("Access token secret was not provided");
        }
        
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = config["AuthTokens:Audience"],
            ValidIssuer = config["AuthTokens:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret)),
            ValidateLifetime = true
        };
        
        serviceCollection.AddAuthentication(opt =>
        {
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.SaveToken = true;
            opt.RequireHttpsMetadata = false;
            opt.MapInboundClaims = false;
            opt.TokenValidationParameters = tokenValidationParameters;
            
            opt.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access-token"];
                    var path = context.HttpContext.Request.Path;
                    
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/game-hub"))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });
    }
}