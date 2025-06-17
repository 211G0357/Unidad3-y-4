using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.Text.RegularExpressions;

namespace RestauranteApi.Models.Validators
{
    public class UsuarioValidator
    {
        private readonly Repository<Usuario> repository;

        public UsuarioValidator(Repository<Usuario> repository)
        {
            this.repository = repository;
        }


        public bool Validate(UsuarioDTO user, out List<string> errores)
        {
            errores = new List<string>();

            if (string.IsNullOrWhiteSpace(user.Nombre))
            {
                errores.Add("El nombre de usuario está vacio.");
            }

            if (string.IsNullOrWhiteSpace(user.Contraseña))
            {
                errores.Add("La contraseña esta vacia");
            }

            if (user.Nombre.Length > 45)
            {
                errores.Add("El nombre de usuario debe tener una longitud máxima de 45 caracteres.");
            }

            if (user.Contraseña.Length > 12)
            {
                errores.Add("La contraseña debe tener una longitud máxima de 12 caracteres.");
            }

            if (repository.GetAll().Any(x => x.Nombre == user.Nombre))
            {
                errores.Add("Ya existe un usuario con el mismo nombre");
            }

            if (!Regex.IsMatch(user.Contraseña, @"^(?=.*[A-ZÑ])(?=.*[a-zñ]).*$"))
            {
                errores.Add("La contraseña debe tener al menos una letra mayusculas y una letra minuscula.");
            }

            return errores.Count == 0;

        }
    }
}
