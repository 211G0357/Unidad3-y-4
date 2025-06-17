using FluentValidation;
using RestauranteApi.Models.DTOs;

public class ListaTicketsDTOValidator : AbstractValidator<ListaTicketsDTO>
{
    public ListaTicketsDTOValidator()
    {
        RuleFor(x => x.NumMesa)
            .GreaterThan(0).WithMessage("La mesa debe ser mayor que cero");
    }
}