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

        public IActionResult Registrar(UsuarioDTO dto)
        {
            if (Validator.Validate(dto, out List<string> errores))
            {
                Usuario user = new()
                {
                    Contraseña = dto.Contraseña,
                    Nombre = dto.Nombre,
                    Rol=dto.Rol ?? "Mesero"


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
            var token=Service.GenerarToken(dto);
            if (token == null)
            {
                return Unauthorized("El Usuario o contraseña son incorrectos");
            }
            
            return Ok(token);
            
        }
    }
}
