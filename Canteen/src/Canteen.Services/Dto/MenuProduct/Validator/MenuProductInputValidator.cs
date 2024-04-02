using Canteen.Services.Services;
using FluentValidation;

namespace Canteen.Services.Dto.Validator;

public class MenuProductInputValidator : CoreValidator<MenuProductOutputDto>
{
    public MenuProductInputValidator()
    {
        RuleFor(f => f.Quantity)
            .NotEqual(0)
            .WithMessage("Quantity must be bigger than 0");
    }
}