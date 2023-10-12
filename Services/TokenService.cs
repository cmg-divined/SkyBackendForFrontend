using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Coflnet.Sky.Commands.Shared;

/// <summary>
/// Handles creation and validation of custom tokens
/// </summary>
public class TokenService
{
    private IConfiguration config;
    private ILogger<TokenService> logger;

    public TokenService(IConfiguration config, ILogger<TokenService> logger)
    {
        this.config = config;
        this.logger = logger;
    }

    /// <summary>
    /// Creates a new token for the given user
    /// </summary>
    /// <param name="email">The user to create the token for</param>
    /// <returns>The token</returns>
    public string CreateToken(string email)
    {
        SigningCredentials creds = GetCreds();
        var randomTokenId = Guid.NewGuid().ToString();

        var token = new JwtSecurityToken(
            issuer: config["JWT_ISSUER"],
            audience: config["JWT_AUDIENCE"],
            claims: new[] { new Claim("email", email), new Claim("tokenId", randomTokenId) },
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private SigningCredentials GetCreds()
    {
        var seed = System.Text.Encoding.UTF8.GetBytes(config["JWT_SECRET"] ?? throw new Exception("JWT_SECRET environment variable not set"));
        var hmac = new System.Security.Cryptography.HMACSHA256(seed);
        var key = new SymmetricSecurityKey(hmac.ComputeHash(seed));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        return creds;
    }

    /// <summary>
    /// Validates the given token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>The claims of the token</returns>
    /// <exception cref="SecurityTokenException">Thrown if the token is invalid</exception>
    public ClaimsPrincipal ValidateToken(string token)
    {
        var creds = GetCreds();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = creds.Key,
            ValidateIssuer = true,
            ValidIssuer = config["JWT_ISSUER"],
            ValidateAudience = true,
            ValidAudience = config["JWT_AUDIENCE"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        return principal;
    }

    /// <summary>
    /// Gets the email of the user from the token
    /// </summary>
    /// <param name="token">The token to get the email from</param>
    /// <returns>The email of the user</returns>
    /// <exception cref="SecurityTokenException">Thrown if the token is invalid</exception>
    public string GetEmailFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal.FindFirst(c=>c.Type == "email" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
    }

    /// <summary>
    /// Tries to get the email from a token string
    /// </summary>
    /// <param name="token">The token to get the email from</param>
    /// <param name="email">The email of the user</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown if the token is invalid</exception>
    public bool TryGetEmailFromToken(string token, out string email)
    {
        try
        {
            email = GetEmailFromToken(token);
            return true;
        }
        catch(Microsoft.IdentityModel.Tokens.SecurityTokenMalformedException e)
        {
            logger.LogError(e, "Token format of token {token} is invalid", token);
            email = null;
            return false;
        }
        catch (SecurityTokenException)
        {
            email = null;
            return false;
        }
    }
}
