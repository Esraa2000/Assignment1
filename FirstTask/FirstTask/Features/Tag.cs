using System.Text.Json;

namespace FirstTask.Endpoints
{
    public static class Tag
    {
        public static void MapTagEndpoints(this WebApplication app)
        {
            app.MapGet("/api/tags", async () =>
            {
                var tagsPath = Path.Combine(Directory.GetCurrentDirectory(), "content", "tags");

                if (!Directory.Exists(tagsPath))
                    return Results.NotFound("Tags folder not found.");

                var tags = Directory.GetFiles(tagsPath, "*.json")
                    .Select(file => JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(file)))
                    .Where(tag => tag != null)
                    .ToList();

                return Results.Ok(tags);
            });
        }
    }
}
