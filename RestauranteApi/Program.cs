using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Models.Validators;
using RestauranteApi.Repositories;
using RestauranteApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
{
    jwtOptions.Audience = builder.Configuration["Jwt:Audience"];
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = true
    };
});

var cs = builder.Configuration.GetConnectionString("Hamburguesa");

//INYECCIÓN DE DEPENDENCIAS (DI)
builder.Services.AddDbContext<HamburguesaContext>(x =>
x.UseMySql(cs, ServerVersion.AutoDetect(cs)));
//builder.Services.AddSingleton AddScoped AddTransient

builder.Services.AddScoped(typeof(Repository<>), typeof(Repository<>));
builder.Services.AddTransient<JwtService>();

//Validadores
builder.Services.AddSignalR();
builder.Services.AddTransient<UsuarioValidator>();
builder.Services.AddScoped<TicketDetalleValidator>();
builder.Services.AddScoped<IValidator<ListaTicketsDTO>, ListaTicketsDTOValidator>();
builder.Services.AddControllers();
builder.Services.AddCors(x =>
{
    x.AddPolicy("todos",
        policy =>
        {
            policy
                .WithOrigins("https://localhost:44349", "https://10.1.199.22:45455", "https://10.1.195.177:45456")
                .AllowAnyMethod()
                .AllowAnyHeader()
            .AllowCredentials();
        });
});

var app = builder.Build();
app.UseRouting();

app.UseCors("todos");

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", context =>
{
    context.Response.Redirect("/Login.html");
    return Task.CompletedTask;
});

app.MapHub<PedidosHub>("/pedidosHub");
app.MapControllers();
app.UseFileServer();
app.Run();
