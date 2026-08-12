using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VitalApp_API.Hubs;
using VitalApp_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Dependencias
builder.Services.AddScoped<IUtilsService, UtilsService>();
builder.Services.AddSignalR();

// CORS para permitir la conexion del chat desde el WEB
builder.Services.AddCors(options =>
{
    var origins = builder.Configuration["Cors:Origins"]!
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    options.AddPolicy("ChatPolicy", policy =>
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Autenticacion con JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero
        };

        // El WebSocket no envia headers; el token viaja como query param
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware de errores
app.UseExceptionHandler("/api/Error/RegisterError");

app.UseHttpsRedirection();

app.UseCors("ChatPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
