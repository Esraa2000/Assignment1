namespace FirstTask.Endpoints
{
    public static class Search
    {
        public static void MapSearchEndpoints(this WebApplication app)
        {
            app.MapGet("/api/search/{search}", HandleSearchRequest);
        }
        private static Task<IResult> HandleSearchRequest(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return Task.FromResult(Results.BadRequest("Search keyword is missing.") as IResult);
            var posts = Post.GetAllPosts();
            var keyword = search.ToLower();
            var matchedPosts = new List<Dictionary<string, object>>();
            foreach (var p in posts)
            {
                var title = p.ContainsKey("title") ? p["title"]?.ToString()?.ToLower() : "";
                var description = p.ContainsKey("description") ? p["description"]?.ToString()?.ToLower() : "";
                var Content = p.ContainsKey("Content") ? p["Content"]?.ToString()?.ToLower() : "";

                if ((title != null && title.Contains(keyword)) ||
                    (description != null && description.Contains(keyword)) ||
                    (Content != null && Content.Contains(keyword)))
                {
                    matchedPosts.Add(p);
                }
            }
            if (matchedPosts.Count == 0)
                return Task.FromResult(Results.NotFound("No posts matched the keyword") as IResult);
            return Task.FromResult(Results.Ok(matchedPosts) as IResult);
        }
    }
}
