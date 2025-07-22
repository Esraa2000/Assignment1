using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FirstTask.Features
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/login", LoginAsync);
        }

        private static async Task<IResult> LoginAsync(LoginRequest request)
        {
            var userFolder = Path.Combine("Content", "users", request.Username);
            var profilePath = Path.Combine(userFolder, "profile.json");

            if (!File.Exists(profilePath))
                return Results.Unauthorized();

            var json = await File.ReadAllTextAsync(profilePath);
            var user = JsonSerializer.Deserialize<UserProfile>(json);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var token = GenerateJwtToken(user.Username, user.Roles);

            return Results.Ok(new { token });
        }

        private static string GenerateJwtToken(string username, string[] roles)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role,"Author")
        };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey1234567890!@#$%^&*()ABCDEF"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Blog",
                audience: "Blog",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public record LoginRequest(string Username, string Password);

        public class UserProfile
        {
            public string Username { get; set; } = "";
            public string PasswordHash { get; set; } = "";
            public string[] Roles { get; set; } = Array.Empty<string>();
        }
    }
}
