
using FirstTask.Endpoints;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapCategoryEndpoints();
app.MapTagEndpoints();
app.MapSearchEndpoints();
app.MapPostEndpoints();
app.UseStaticFiles();


app.Run();



