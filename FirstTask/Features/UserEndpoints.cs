using System.Text.Json;
using BCrypt.Net;

namespace FirstTask.Features;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/users/register", RegisterUserAsync);
    }

    private static async Task<IResult> RegisterUserAsync(UserRegisterRequest request)
    {
        var userFolder = Path.Combine("Content", "users", request.Username);
        if (Directory.Exists(userFolder))
            return Results.Conflict("User already exists.");

        Directory.CreateDirectory(userFolder);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var userData = new UserProfile
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Roles = new[] { "Author" }  
        };

        var json = JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(userFolder, "profile.json"), json);

        return Results.Ok(new { message = "User registered successfully." });
    }

    public record UserRegisterRequest(string Username, string Password);

    public class UserProfile
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
