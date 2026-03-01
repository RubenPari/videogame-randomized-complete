using FluentValidation;
using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Validators;

public class CreateGameValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Rating).InclusiveBetween(0, 5);
        RuleFor(x => x.Released).Matches(@"^\d{4}-\d{2}-\d{2}$").When(x => !string.IsNullOrEmpty(x.Released))
            .WithMessage("Released date must be in YYYY-MM-DD format");
        
        // Add more rules as needed
    }
}

public class UpdateGameValidator : AbstractValidator<UpdateGameDto>
{
    public UpdateGameValidator()
    {
        RuleFor(x => x.PersonalRating).InclusiveBetween(1, 5).When(x => x.PersonalRating.HasValue);
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
