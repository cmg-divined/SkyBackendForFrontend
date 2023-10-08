using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Coflnet.Sky.Commands.Shared;

/// <summary>
/// Handles creation and validation of custom tokens
/// </summary>
public class TokenService
{
    private IConfiguration config;

    public TokenService(IConfiguration config)
    {
        this.config = config;
    }

    /// <summary>
    /// Creates a new token for the given user
    /// </summary>
    /// <param name="email">The user to create the token for</param>
    /// <returns>The token</returns>
    public string CreateToken(string email)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["JWT_KEY"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var randomTokenId = Guid.NewGuid().ToString();

        var token = new JwtSecurityToken(
            issuer: config["JWT_ISSUER"],
            audience: config["JWT_AUDIENCE"],
            claims: new[] { new Claim("email", email), new Claim("tokenId", randomTokenId) },
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates the given token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>The claims of the token</returns>
    /// <exception cref="SecurityTokenException">Thrown if the token is invalid</exception>
    public ClaimsPrincipal ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["JWT_KEY"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
        return principal.FindFirst("email").Value;
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
        catch (SecurityTokenException)
        {
            email = null;
            return false;
        }
    }
}
