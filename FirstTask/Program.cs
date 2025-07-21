
using FirstTask.Endpoints;
using FirstTask.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "your-app",
            ValidAudience = "your-app",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey1234567890!@#$%^&*()ABCDEF"))
        };
    });

builder.Services.AddAuthorization(options => { options.AddPolicy("Author", policy => policy.RequireRole("Author")); });
 
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Content")),
    RequestPath = "/Content"
});

app.UseAuthentication();
app.UseAuthorization();


app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapCategoryEndpoints();
app.MapTagEndpoints();
app.MapSearchEndpoints();
app.MapPostEndpoints();






app.Run();



