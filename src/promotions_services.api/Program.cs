using Microsoft.EntityFrameworkCore;
using promotions_services.infrastructure.Persistence.Context;
using System.Reflection;
using promotions_services.application.Commands.Command;
using promotions_services.domain.Interfaces;
using promotions_services.infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// crear variable para la cadena de conexion
var connectionString = builder.Configuration.GetConnectionString("ConnectionPostgre"); //ConnectionPostgre es el parametro de conexion que creamos en el appsetting
//registrar servicio para la conexion


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString,
        b => b.MigrationsAssembly("promotions_services.infrastructure")));


builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GenerateCouponCommand).Assembly));

builder.Services.AddSignalR();

builder.Services.AddScoped<ICouponRepository, CouponRepository>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Obtiene el DbContext
        var context = services.GetRequiredService<AppDbContext>();

        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones a la base de datos.");
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowLocalhost3000");

app.UseAuthorization();

app.MapControllers();

app.Run();

