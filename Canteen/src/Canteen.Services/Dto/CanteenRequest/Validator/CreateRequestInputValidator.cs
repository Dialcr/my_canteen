using Canteen.Services.Services;
using FluentValidation;

namespace Canteen.Services.Dto.CanteenRequest.Validator;

public class CreateRequestInputValidator : CoreValidator<EditRequestDto>
{
    public CreateRequestInputValidator()
    {
        RuleFor(f => f.DeliveryDate)
            .NotNull()
            .NotEmpty()
            .WithMessage("Delivery date must be set")
            .GreaterThan(DateTime.Now).WithMessage("Delivery date must be in the future");

        RuleFor(f => f.DeliveryLocation)
            .NotNull()
            .NotEmpty()
            .WithMessage("Delivery location must be set");
        RuleFor(f => f.Products)
            .NotEmpty()
            .NotEmpty()
            .Must(x=>x.Any())
            .WithMessage("Products must be set");
    }
}