using FlightBookingAPI.Models;
using FlightBookingAPI.Services;
using FlightBookingAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configurar el puerto dinámico para Railway/Producción
var port = Environment.GetEnvironmentVariable("PORT") ?? "5050";
builder.WebHost.UseUrls($"http://*:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings")
);

builder.Services.AddSingleton<AuthService>();

// CORS modificado para aceptar tanto local como tu futuro despliegue
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // Local Next.js
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true); // Esto permite que Vercel se conecte sin trabas de CORS
    });
});

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

// Cambia AddSingleton por AddTransient
builder.Services.AddTransient<EmailService>();

var app = builder.Build();

// CONFIGURACIÓN SEGURA:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlightBookingAPI v1.");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();

app.Run();