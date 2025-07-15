using System.Text.Json;

namespace FirstTask.Endpoints
{
    public static class Category
    {
        public static void MapCategoryEndpoints(this WebApplication app)
        {
            app.MapGet("/api/categories", GetAllCategories);
        }
        private static IResult GetAllCategories(HttpContext context)
        {
            var categoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Content", "categories");

            if (!Directory.Exists(categoryPath))
                return Results.NotFound("Categories folder not found.");

            var categoryFiles = Directory.GetFiles(categoryPath, "*.json");
            var categories = new List<Dictionary<string, object>>();

            foreach (var file in categoryFiles)
            {
                var json = File.ReadAllText(file);
                var category = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (category != null)
                {
                    categories.Add(category);
                }
            }

            return Results.Ok(categories);
        }


    }
}
