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
            app.MapPost("/api/posts", CreatePostAsync); 
        }

        public static async Task<IResult> CreatePostAsync(HttpRequest request)
        {

            try
            {
                var postData = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(request.Body);

                if (postData == null || !postData.ContainsKey("title") || !postData.ContainsKey("Content"))
                    return Results.BadRequest("Title and Content are required.");

                var slug = postData["title"]?.ToString()?.ToLower()?.Replace(" ", "-");
                if (string.IsNullOrWhiteSpace(slug))
                    return Results.BadRequest("Invalid title.");

                var postFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts", slug);
                Directory.CreateDirectory(postFolder);

                // Save meta.json
                var metaData = new Dictionary<string, object>
                {
                    ["title"] = postData["title"],
                    ["slug"] = slug,
                    ["description"] = postData.ContainsKey("description") ? postData["description"] : "",
                    ["categories"] = postData.ContainsKey("category") ? new[] { postData["category"]?.ToString() } : [],
                    ["tags"] = postData.ContainsKey("tags") && postData["tags"] is JsonElement tagsElem && tagsElem.ValueKind == JsonValueKind.Array
                        ? tagsElem.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrEmpty(x)).ToArray()
                        : [],
                    ["publishedDate"] = postData.ContainsKey("publishedDate") ? postData["publishedDate"]?.ToString() : DateTime.UtcNow.ToString(),
                    ["lastModified"] = DateTime.UtcNow.ToString(),
                    ["status"] = postData.ContainsKey("published") && postData["published"]?.ToString()?.ToLower() == "true" ? "published" : "draft"
                };

                var metaJson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Path.Combine(postFolder, "meta.json"), metaJson);

                // Save Content.md
                var content = postData["content"]?.ToString() ?? "";
                await File.WriteAllTextAsync(Path.Combine(postFolder, "content.md"), content);

                return Results.Ok(new { message = "Post created successfully", slug = slug });
            }
            catch (Exception ex)
            {
                return Results.Problem("Error creating post: " + ex.Message);
            }
        }


        private static Dictionary<string, object>? ReadPost(string folder)
        {
            var metaPath = Path.Combine(folder, "meta.json");
            var ContentFile = Path.Combine(folder, "content.md");

            if (File.Exists(metaPath) && File.Exists(ContentFile))
            {
                var metaJson = File.ReadAllText(metaPath);
                var Content = File.ReadAllText(ContentFile);
                var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);
                if (metaData != null)
                {
                    var ContentHtml = Markdown.ToHtml(Content);
                    metaData["content"] = ContentHtml;
                    return metaData;
                }
            }

            return null;
        }

        internal static List<Dictionary<string, object>> GetAllPosts()
        {
            var postDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts");
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
            var postDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts");

            if (!Directory.Exists(postDirectory))
                return Results.NotFound("Posts directory not found.");

            var postFolders = Directory.GetDirectories(postDirectory);
            var posts = new List<object>();

            foreach (var folder in postFolders)
            {
                var metaPath = Path.Combine(folder, "meta.json");
                var ContentFile = Path.Combine(folder, "content.md");

                if (File.Exists(metaPath) && File.Exists(ContentFile))
                {
                    var metaJson = File.ReadAllText(metaPath);
                    var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);

                    if (metaData != null &&
                       metaData.ContainsKey("categories") &&
                       metaData["categories"] is JsonElement catElement &&
                       catElement.ValueKind == JsonValueKind.Array &&
                       catElement.EnumerateArray().Any(c => c.GetString()?.Equals(categoryName, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        var Content = File.ReadAllText(ContentFile);
                        metaData["Content"] = Content;

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
            var postDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts");

            if (!Directory.Exists(postDirectory))
                return Results.NotFound("Not found");

            var postFolders = Directory.GetDirectories(postDirectory);
            var posts = new List<object>();

            foreach (var folder in postFolders)
            {
                var metaPath = Path.Combine(folder, "meta.json");
                var ContentFile = Path.Combine(folder, "content.md");

                if (File.Exists(metaPath) && File.Exists(ContentFile))
                {
                    var metaJson = File.ReadAllText(metaPath);
                    var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);

                    if (metaData != null &&
                       metaData.ContainsKey("tags") &&
                       metaData["tags"] is JsonElement tagElement &&
                       tagElement.ValueKind == JsonValueKind.Array &&
                       tagElement.EnumerateArray().Any(t => t.GetString()?.Equals(tagName, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        var Content = File.ReadAllText(ContentFile);
                        metaData["Content"] = Content;

                        posts.Add(metaData);
                    }
                }
            }

            return Results.Ok(posts);
        }
    }
}
