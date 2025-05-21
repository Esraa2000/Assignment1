namespace FirstTask.Endpoints
{
    public static class Search
    {
        public static void MapSearchEndpoints(this WebApplication app)
        {
            app.MapGet("/api/search/{search}", async (string search) =>
            {
                var posts = await Task.FromResult(Post.GetAllPosts());

                var keyword = search.ToLower();

                var matchedPosts = posts.Where(p =>
                {
                    var title = p.ContainsKey("title") ? p["title"]?.ToString()?.ToLower() : "";
                    var description = p.ContainsKey("description") ? p["description"]?.ToString()?.ToLower() : "";
                    var content = p.ContainsKey("content") ? p["content"]?.ToString()?.ToLower() : "";

                    return title.Contains(keyword) || description.Contains(keyword) || content.Contains(keyword);
                }).ToList();

                if (matchedPosts.Count == 0)
                    return Results.NotFound("No posts matched the keyword");

                return Results.Ok(matchedPosts);
            });
        }
    }
}
