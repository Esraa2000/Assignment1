using System.Text.Json;
using Markdig;

namespace FirstTask.Endpoints
{
    public static class Post
    {
        public static void MapPostEndpoints(this WebApplication app)
        {
            app.MapGet("/api/posts", GetAllPostsAsync);
            app.MapGet("/api/posts/{slug}", GetPostBySlugAsync);
            app.MapGet("/api/posts/category/{categoryName}", GetPostsByCategoryAsync);
            app.MapGet("/api/posts/tag/{tagName}", GetPostsByTagAsync);
        }
        private static Dictionary<string, object>? ReadPost(string folder)
        {
            var metaPath = Path.Combine(folder, "meta.json");
            var contentFile = Path.Combine(folder, "content.md");

            if (File.Exists(metaPath) && File.Exists(contentFile))
            {
                var metaJson = File.ReadAllText(metaPath);
                var content = File.ReadAllText(contentFile);
                var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);
                if (metaData != null)
                {
                    var contentHtml = Markdown.ToHtml(content);
                    metaData["content"] = contentHtml; 
                    return metaData;
                }
            }

            return null;
        }

        internal static List<Dictionary<string, object>> GetAllPosts()
        {
            var postDirectory = Path.Combine(Directory.GetCurrentDirectory(), "content", "posts");
            var posts = new List<Dictionary<string, object>>();

            if (Directory.Exists(postDirectory))
            {
                foreach (var folder in Directory.GetDirectories(postDirectory))
                {
                    var post = ReadPost(folder);
                    if (post != null)
                        posts.Add(post);
                }
            }

            return posts;
        }
        public static async Task<IResult> GetAllPostsAsync()
        {
            
            var posts = await Task.FromResult(GetAllPosts());

            if (posts.Count == 0)
                return Results.NotFound("No posts found");

            return Results.Ok(posts);
        }
        public static async Task<IResult> GetPostBySlugAsync(string slug)
        {
            var posts = await Task.FromResult(GetAllPosts());
            var post = posts.FirstOrDefault(p => p.ContainsKey("slug") && p["slug"]?.ToString() == slug);

            if (post == null)
                return Results.NotFound($"Post with slug '{slug}' not found.");

            return Results.Ok(post);
        }

        public static async Task<IResult> GetPostsByCategoryAsync(string categoryName)
        {
            var postDirectory = Path.Combine(Directory.GetCurrentDirectory(), "content", "posts");

            if (!Directory.Exists(postDirectory))
                return Results.NotFound("Posts directory not found.");

            var postFolders = Directory.GetDirectories(postDirectory);
            var posts = new List<object>();

            foreach (var folder in postFolders)
            {
                var metaPath = Path.Combine(folder, "meta.json");
                var contentFile = Path.Combine(folder, "content.md");

                if (File.Exists(metaPath) && File.Exists(contentFile))
                {
                    var metaJson = File.ReadAllText(metaPath);
                    var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);

                    if (metaData != null &&
                       metaData.ContainsKey("categories") &&
                       metaData["categories"] is JsonElement catElement &&
                       catElement.ValueKind == JsonValueKind.Array &&
                       catElement.EnumerateArray().Any(c => c.GetString()?.Equals(categoryName, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        var content = File.ReadAllText(contentFile);
                        metaData["content"] = content;

                        posts.Add(metaData);
                    }
                }
            }

            if (posts.Count == 0)
                return Results.NotFound($"No posts found in category '{categoryName}'.");

            return Results.Ok(posts);
        }

        public static async Task<IResult> GetPostsByTagAsync(string tagName)
        {
            var postDirecory = Path.Combine(Directory.GetCurrentDirectory(), "content", "posts");


            if (!Directory.Exists(postDirecory))
                return Results.NotFound("Not found");

            var PostFolder = Directory.GetDirectories(postDirecory);
            var posts = new List<object>();

            foreach (var folder in PostFolder)
            {
                var metaPath = Path.Combine(folder, "meta.json");
                var contentFile = Path.Combine(folder, "content.md");

                if (File.Exists(metaPath) && File.Exists(contentFile))
                {
                    var metaJson = File.ReadAllText(metaPath);

                    var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);

                    if (metaData != null &&
                       metaData.ContainsKey("tags") &&
                       metaData["tags"] is JsonElement catElement &&
                       catElement.ValueKind == JsonValueKind.Array &&
                       catElement.EnumerateArray().Any(c => c.GetString()?.Equals(tagName, StringComparison.OrdinalIgnoreCase) == true))
                    {

                        var content = File.ReadAllText(contentFile);
                        metaData["content"] = content;

                        posts.Add(metaData);
                    }

                }
            }
            return Results.Ok(posts);
           
        }



    }
}
