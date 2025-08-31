using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rozetochka_api.Application.Users.Repository;
using rozetochka_api.Application.Users.Service;
using rozetochka_api.Infrastructure.Data;
using rozetochka_api.Infrastructure.Identity;
using rozetochka_api.Infrastructure.Identity.Interfaces;
using rozetochka_api.Shared;
using Swashbuckle.AspNetCore.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


/*  Documentation:
    
    - в Azure portal, SQL Server в Бзопасность -> Сеть  добавляли свой IP adreess, со временем свой ip может изменится и надо будет обновить свой ip?
    - Надо создать свой файл appsettings-Secrets.json и определить его по структуре как описана в appsettings-Secrets.sample.json  ( там секреты + ДБ стринг).
    
    ###
    - отключил автоматическое! поведение (!ModelState.IsValid) [ApiController] (SuppressModelStateInvalidFilter) при невалидной модели, чтобы вручную вернуть RestResponse вместо ProblemDetails.
 
    
 */

/*
   TODO:
    - CORS ограничить после деплоя фронта   (WithOrigins(...).AllowCredentials())
    - Убрать сваггер с прода в конце разработки. И тест контроллер.


    TODO Never:
    - метод _userRepository.RevokeRefreshTokenAsync не удаляет refresh-токены, старые revoked-токены будут копиться в БД. Очистку нужно выполнять отдельной задачей (напр. раз в месяц: IsRevoked = true && ExpiresAt < Now).

 */


//--------------------------------------------------------------------------------

// Swagger
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rozetochka API", Version = "v1" });
    c.ExampleFilters();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT (without 'Bearer ')"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

//--------------------------------------------------------------------------------

//  Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        {
            policy
            .WithOrigins(
                "https://brave-smoke-0606f8503.1.azurestaticapps.net", 
                "http://localhost:5173"
            )
            //.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });

});

//--------------------------------------------------------------------------------

// Config (DB connection string, секреты)
builder.Configuration.AddJsonFile("appsettings-Secrets.json", optional: true, reloadOnChange: true);

// DbContext  ( + CommandTimeout )
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            sql.CommandTimeout(30);     // максимальное время ожидания выполнения SQL запроса секунд
        });
    
    // EF Core подробные ошибки и параметры sql запросов
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

//--------------------------------------------------------------------------------

// Отключаем авто-400 от [ApiController] при невалидной модели (!ModelState.IsValid),
// чтобы вручную вернуть наш RestResponse вместо ProblemDetails.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//--------------------------------------------------------------------------------

// DI services

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService,    UserService>();

builder.Services.AddAutoMapper(typeof(Program));        // AutoMapper




// JWT auth
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;        // отключение автомапинга клеймов .net ? чтобы небыло сюрпризов?

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keyBase64 = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT:Key is missing");
        var keyBytes = Convert.FromBase64String(keyBase64!);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer    = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience    = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

//--------------------------------------------------------------------------------

var app = builder.Build();

//--------------------------------------------------------------------------------

// Глобальный обработчик необработанных ошибок.
// Логирует исключение и возвращает RestResponse в едином формате (без деталей в Prod)
// ставим его самым первым в пайплайне, сразу после var app = builder.Build()
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var env     = context.RequestServices.GetRequiredService<IHostEnvironment>();
        var logger  = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(ex, "Unhandled exception {Method} {Path}", context.Request.Method, context.Request.Path);

        var (status, phrase, errorCode) = ex switch
        {
            ArgumentException           => (400, "Bad Request", "INVALID_ARGUMENT"),
            InvalidOperationException   => (400, "Bad Request", "INVALID_OPERATION"),
            UnauthorizedAccessException => (401, "Unauthorized", "UNAUTHORIZED"),
            KeyNotFoundException        => (404, "Not Found", "NOT_FOUND"),
            TaskCanceledException       => (499, "Client Closed Request", "REQUEST_CANCELLED"),     // TODO, canceletion token не делал.
            _                           => (500, "Internal Server Error", "INTERNAL_ERROR")
        };

        var errorData = new Dictionary<string, object?>
        {
            ["errorCode"] = errorCode
        };
        if (env.IsDevelopment())    // в Dev больше деталей, могут быть чувствительные данные. В проде минимум?
        {
            errorData["message"]    = ex?.Message;
            errorData["stackTrace"] = ex?.StackTrace;
        }

        var response = new RestResponse
        {
            Status = new RestStatus { IsOk = false, Code = status, Phrase = phrase },
            Meta = new RestMeta
            {
                Service = "rozetochka-api",
                Method = context.Request.Method,
                Action = context.Request.Path,
                DataType = "dictionary",
                Params = null
            },
            Data = errorData
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(response);
    });
});

//--------------------------------------------------------------------------------

// Проверяем наличие локального файла с секретами
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
        db.Database.Migrate();  // применяет существующие миграции к бд (Update-Database) но предварительно надо Add-Migration делать вручную.
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "STARTUP FAILURE: DB migrate failed. (program.cs - DB Scope)");
    }
}

//--------------------------------------------------------------------------------

// Pipeline
//if (app.Environment.IsDevelopment())
//{
//    // Swagger
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.RoutePrefix = ""; // SwaggerUI доступен на корневрм URL "/"
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rozetochka API v1");

//    });
//}

// Убрал из if(), теперь и на проде в Azure. потом вернуть.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rozetochka API v1");
    c.RoutePrefix = "";
});

//--------------------------------------------------------------------------------

app.UseHttpsRedirection();
app.UseCors();              // +

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();
