namespace Canteen.Services.Services;

using FluentValidation;

public class CoreValidator<TDto> : AbstractValidator<TDto> where TDto : class
{
    protected CoreValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}