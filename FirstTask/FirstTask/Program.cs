
using FirstTask.Endpoints;



var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();


app.MapCategoryEndpoints();
app.MapTagEndpoints();
app.MapSearchEndpoints();
app.MapPostEndpoints();





app.Run();


