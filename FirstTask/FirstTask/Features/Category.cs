using System.Text.Json;

namespace FirstTask.Endpoints
{
    public static class Category
    {
        public static void MapCategoryEndpoints(this WebApplication app)
        {
            app.MapGet("/api/categories", async () =>
            {
                var categoryPath = Path.Combine(Directory.GetCurrentDirectory(), "content", "categories");

                if (!Directory.Exists(categoryPath))
                    return Results.NotFound("Categories folder not found.");

                var categories = Directory.GetFiles(categoryPath, "*.json")
                    .Select(file => JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(file)))
                    .Where(category => category != null)
                    .ToList();

                return Results.Ok(categories);
            });
        }

       
    }
}
