using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using rozetochka_api.Application.Users.Repository;
using rozetochka_api.Application.Users.Service;
using rozetochka_api.Infrastructure.Data;
using rozetochka_api.Infrastructure.Identity;
using rozetochka_api.Infrastructure.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using rozetochka_api.Application.Users.DTOs.Examples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


/*
   TODO:
    - CORS ���������� ����� ������ ������   (WithOrigins(...).AllowCredentials())?


 */


//--------------------------------------------------------------------------------

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rozetochka API", Version = "v1" });
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<UserRegisterRequestExample>();

//--------------------------------------------------------------------------------

//  Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

//--------------------------------------------------------------------------------

// Config (DB connection string, �������)
builder.Configuration.AddJsonFile("appsettings-Secrets.json", optional: true, reloadOnChange: true);

// DbContext  ( + CommandTimeout )
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            sql.CommandTimeout(30);     // ������������ ����� �������� ���������� SQL ������� ������
        });
    
    // EF Core ��������� ������ � ��������� sql ��������
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

//--------------------------------------------------------------------------------

// ��������� ����-400 �� [ApiController] ��� ���������� ������ (!ModelState.IsValid),
// ����� ������� ������� ��� RestResponse ������ ProblemDetails.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//--------------------------------------------------------------------------------

// DI services

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAutoMapper(typeof(Program));        // AutoMapper

//--------------------------------------------------------------------------------

var app = builder.Build();


// ��������� ������� ���������� ����� � ���������
var secretsFilePath = "appsettings-Secrets.json";
if (!File.Exists(secretsFilePath))
{
    app.Logger.LogWarning("Configuration file '{FileName}' not found.", secretsFilePath);
}

//--------------------------------------------------------------------------------

// DB scope
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // db.Database.Migrate();  // ��������� ������������ �������� � �� (Update-Database)
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "STARTUP FAILURE: DB migrate failed. (program.cs - DB Scope)");
    }
}

//--------------------------------------------------------------------------------

// Pipeline
if (app.Environment.IsDevelopment())
{
    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = string.Empty; // SwaggerUI �������� �� �������� URL "/"
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rozetochka API v1");
    
    });
}

app.UseHttpsRedirection();
app.UseCors();  // +
app.UseAuthorization();
app.MapControllers();



app.Run();
