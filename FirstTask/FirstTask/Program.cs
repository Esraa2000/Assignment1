using FirstTask.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Content")),
    RequestPath = "/Content"
});

app.MapCategoryEndpoints();
app.MapTagEndpoints();
app.MapSearchEndpoints();
app.MapPostEndpoints();

app.Run();


