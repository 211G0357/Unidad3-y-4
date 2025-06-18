using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<IValidator<ListaTicketsDTO>, ListaTicketsDTOValidator>();
builder.Services.AddScoped<TicketDetalleValidator>();
builder.Services.AddControllers();
builder.Services.AddCors(x =>
{
    x.AddPolicy("todos", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();
app.MapHub<PedidosHub>("/pedidosHub");
app.MapControllers();
app.UseFileServer();
app.Run();
