using Markdig;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

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
            app.MapDelete("/api/posts/{slug}", DeletePostBySlugAsync);
            app.MapPut("/api/posts/{slug}", UpdatePostAsync);



        }
        public static async Task<IResult> UpdatePostAsync(string slug, HttpRequest request)
        {
            try
            {
                if (!request.HasFormContentType)
                    return Results.BadRequest("Form content is required.");

                var form = await request.ReadFormAsync();

                var title = form["title"].ToString();
                var content = form["content"].ToString();
                var category = form["category"].ToString();
                var tags = form["tags"].ToString()
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToArray();
                var published = form["published"].ToString().ToLower() == "true";

                var postFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts", slug);
                if (!Directory.Exists(postFolder))
                    return Results.NotFound("Post not found.");

                // تحديث meta.json
                var metaPath = Path.Combine(postFolder, "meta.json");
                if (!File.Exists(metaPath))
                    return Results.NotFound("Post metadata not found.");

                var meta = JsonSerializer.Deserialize<Dictionary<string, object>>(await File.ReadAllTextAsync(metaPath));
                if (meta == null)
                    return Results.BadRequest("Invalid metadata.");

                meta["title"] = title;
                meta["description"] = form["description"].ToString() ?? "";
                meta["categories"] = string.IsNullOrWhiteSpace(category) ? [] : new[] { category };
                meta["tags"] = tags;
                meta["lastModified"] = DateTime.UtcNow.ToString("s");
                meta["status"] = published ? "published" : "draft";

                // تحديث الصورة لو فيه صورة جديدة
                var file = form.Files["image"];
                if (file != null && file.Length > 0)
                {
                    var assetsFolder = Path.Combine(postFolder, "assets");
                    Directory.CreateDirectory(assetsFolder);

                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(assetsFolder, fileName);

                    using var image = await Image.LoadAsync(file.OpenReadStream());
                    if (image.Width > 1024)
                        image.Mutate(x => x.Resize(1024, 0));

                    await image.SaveAsync(filePath);
                    meta["image"] = $"/Content/posts/{slug}/assets/{fileName}";
                }

                // حفظ metadata
                var metaJson = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metaPath, metaJson);

                // تحديث content.md
                await File.WriteAllTextAsync(Path.Combine(postFolder, "content.md"), content);

                return Results.Ok(new { message = "Post updated successfully", slug = slug });
            }
            catch (Exception ex)
            {
                return Results.Problem("Error updating post: " + ex.Message);
            }
        }

        public static async Task<IResult> DeletePostBySlugAsync(string slug)
        {
            try
            {
                var postFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts", slug);
                if (!Directory.Exists(postFolder))
                    return Results.NotFound("Post not found.");
             
                var metaPath = Path.Combine(postFolder, "meta.json");
                if (!File.Exists(metaPath))
                    return Results.Problem("Metadata not found.");

                var metaJson = await File.ReadAllTextAsync(metaPath);
                var meta = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson);

                var categories = meta?["categories"] as JsonElement?;
                var tags = meta?["tags"] as JsonElement?;

                Directory.Delete(postFolder, recursive: true);

                if (categories is JsonElement catElement && catElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var cat in catElement.EnumerateArray())
                    {
                        var catName = cat.GetString();
                        if (!string.IsNullOrWhiteSpace(catName))
                        {
                            var otherPosts = Post.GetAllPosts().Any(p =>
                                p.ContainsKey("categories") &&
                                p["categories"] is JsonElement ce &&
                                ce.EnumerateArray().Any(c => c.GetString()?.Equals(catName, StringComparison.OrdinalIgnoreCase) == true));

                            if (!otherPosts)
                            {
                                var catFile = Path.Combine(Directory.GetCurrentDirectory(), "Content", "categories", $"{catName.ToLower().Replace(" ", "-")}.json");
                                if (File.Exists(catFile))
                                    File.Delete(catFile);
                            }
                        }
                    }
                }
                if (tags is JsonElement tagElement && tagElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagElement.EnumerateArray())
                    {
                        var tagName = tag.GetString();
                        if (!string.IsNullOrWhiteSpace(tagName))
                        {
                            var otherPosts = Post.GetAllPosts().Any(p =>
                                p.ContainsKey("tags") &&
                                p["tags"] is JsonElement te &&
                                te.EnumerateArray().Any(t => t.GetString()?.Equals(tagName, StringComparison.OrdinalIgnoreCase) == true));

                            if (!otherPosts)
                            {
                                var tagFile = Path.Combine(Directory.GetCurrentDirectory(), "Content", "tags", $"{tagName.ToLower().Replace(" ", "-")}.json");
                                if (File.Exists(tagFile))
                                    File.Delete(tagFile);
                            }
                        }
                    }
                }

                return Results.Ok(new { message = $"Post '{slug}' and associated metadata deleted." });
            }
            catch (Exception ex)
            {
                return Results.Problem("Error deleting post: " + ex.Message);
            }
        }

        public static async Task<IResult> CreatePostAsync(HttpRequest request)
        {
            try
            {
                if (!request.HasFormContentType)
                    return Results.BadRequest("Form content is required.");

                var form = await request.ReadFormAsync();

                var title = form["title"].ToString();
                var content = form["content"].ToString();
                var category = form["category"].ToString();
                var tags = form["tags"].ToString()
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToArray();
                var published = form["published"].ToString().ToLower() == "true";

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                    return Results.BadRequest("Title and content are required.");

                var slug = title.ToLower().Replace(" ", "-");
                var postFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "posts", slug);
                Directory.CreateDirectory(postFolder);

                string? imagePath = null;
                var file = form.Files["image"];
                if (file != null && file.Length > 0)
                {
                    var assetsFolder = Path.Combine(postFolder, "assets");
                    Directory.CreateDirectory(assetsFolder);

                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(assetsFolder, fileName);

                    
                    using var image = await Image.LoadAsync(file.OpenReadStream());
                    if (image.Width > 1024)
                        image.Mutate(x => x.Resize(1024, 0));

                    await image.SaveAsync(filePath);
                    imagePath = $"/Content/posts/{slug}/assets/{fileName}";
                }

               
                var metaData = new Dictionary<string, object>
                {
                    ["title"] = title,
                    ["slug"] = slug,
                    ["description"] = form["description"].ToString() ?? "",
                    ["categories"] = string.IsNullOrWhiteSpace(category) ? [] : new[] { category },
                    ["tags"] = tags,
                    ["publishedDate"] = DateTime.UtcNow.ToString("s"),
                    ["lastModified"] = DateTime.UtcNow.ToString("s"),
                    ["status"] = published ? "published" : "draft",
                    ["image"] = imagePath ?? ""
                };

                var metaJson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Path.Combine(postFolder, "meta.json"), metaJson);

                await File.WriteAllTextAsync(Path.Combine(postFolder, "content.md"), content);

               
                if (!string.IsNullOrWhiteSpace(category))
                {
                    var categoryFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "categories");
                    Directory.CreateDirectory(categoryFolder);

                    var catFileName = $"{category.ToLower().Replace(" ", "-")}.json";
                    var categoryPath = Path.Combine(categoryFolder, catFileName);

                    if (!File.Exists(categoryPath))
                    {
                        var categoryData = new Dictionary<string, string>
                        {
                            ["name"] = category,
                            ["created"] = DateTime.UtcNow.ToString("s")
                        };
                        var categoryJson = JsonSerializer.Serialize(categoryData, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(categoryPath, categoryJson);
                    }
                }

             
                if (tags.Length > 0)
                {
                    var tagsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Content", "tags");
                    Directory.CreateDirectory(tagsFolder);

                    foreach (var tag in tags)
                    {
                        var tagFileName = $"{tag.ToLower().Replace(" ", "-")}.json";
                        var tagPath = Path.Combine(tagsFolder, tagFileName);

                        if (!File.Exists(tagPath))
                        {
                            var tagData = new Dictionary<string, string>
                            {
                                ["name"] = tag,
                                ["created"] = DateTime.UtcNow.ToString("s")
                            };
                            var tagJson = JsonSerializer.Serialize(tagData, new JsonSerializerOptions { WriteIndented = true });
                            await File.WriteAllTextAsync(tagPath, tagJson);
                        }
                    }
                }

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
