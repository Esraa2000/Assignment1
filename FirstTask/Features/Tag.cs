using System.Text.Json;

namespace FirstTask.Endpoints
{
    public static class Tag
    {
        public static void MapTagEndpoints(this WebApplication app)
        {
            app.MapGet("/api/tags", () => HandleGetTagsAsync());
        }

        private static Task<IResult> HandleGetTagsAsync()
        {
            var tagsPath = Path.Combine(Directory.GetCurrentDirectory(), "Content", "tags");

            if (!Directory.Exists(tagsPath))
                return Task.FromResult(Results.NotFound("Tags folder not found.") as IResult);

            var tagFiles = Directory.GetFiles(tagsPath, "*.json");
            var tags = new List<Dictionary<string, object>>();

            foreach (var file in tagFiles)
            {
                var Content = File.ReadAllText(file);
                var tag = JsonSerializer.Deserialize<Dictionary<string, object>>(Content);
                if (tag != null)
                {
                    tags.Add(tag);
                }
            }

            return Task.FromResult(Results.Ok(tags) as IResult);
        }
    }
}
