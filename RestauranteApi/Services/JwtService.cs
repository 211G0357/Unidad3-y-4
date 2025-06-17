using Microsoft.IdentityModel.Tokens;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestauranteApi.Services
{
    public class JwtService
    {
        public JwtService(IConfiguration configuration, Repository<Usuario> repository)
        {
            Configuration = configuration;
            Repository = repository;
        }

        public IConfiguration Configuration { get; }
        public Repository<Usuario> Repository { get; }

        public string? GenerarToken(UsuarioDTO dto)
        {
            
            var usuario = Repository.GetAll()
                .FirstOrDefault(x => x.Nombre == dto.Nombre && x.Contraseña == dto.Contraseña);

            if (usuario == null)
            {
                return null;
            }

            
            var claims = new List<Claim>
        {
            new Claim("IdUsuario", usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new JwtSecurityToken(
                issuer: Configuration["Jwt:Issuer"],
                audience: Configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), 
                signingCredentials: creds
            );

           
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(descriptor);
        }
    }
}
