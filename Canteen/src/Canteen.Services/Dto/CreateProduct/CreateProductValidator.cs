using System;
using Canteen.Services.Services;
using FluentValidation;

namespace Canteen.Services.Dto.CreateProduct;

public class CreateProductValidator : CoreValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Name can't be empty.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

    }
}
