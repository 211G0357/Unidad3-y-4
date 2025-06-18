using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Services;
using RestauranteApi.Repositories;
using RestauranteApi.Models.Entities;
using RestauranteApi.Models.Validators;
namespace RestauranteApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        public UsuariosController(Repository<Usuario> repository, UsuarioValidator validator, JwtService service)
        {
            Repository = repository;
            Validator = validator;
            Service = service;
        }

        public Repository<Usuario> Repository { get; }
        public UsuarioValidator Validator { get; }
        public JwtService Service { get; }

        [HttpPost("Registrar")]
        public IActionResult Registrar(UsuarioDTO dto)
        {
            if (Validator.Validate(dto, out List<string> errores))
            {
                Usuario user = new()
                {
                    Contraseña = dto.Contraseña,
                    Nombre = dto.Nombre,
                    
                };
                Repository.Insert(user);
                return Ok();
            }
            else
            {
                return BadRequest(errores);
            }
        }
        [HttpPost("Login")]
        public IActionResult Login(UsuarioDTO dto)
        {
            var token = Service.GenerarToken(dto);
            if (token == null)
            {
                return Unauthorized("Usuario o contraseña incorrectos");
            }

            var usuario = Service.Repository.GetAll()
                          .FirstOrDefault(x => x.Nombre == dto.Nombre);

            return Ok(new
            {
                Token = token,
                Rol = usuario?.Rol,
                Usuario = usuario?.Nombre
            });
        }
    }
}
