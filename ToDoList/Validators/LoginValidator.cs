using FluentValidation;
using ToDoList.Models;

namespace ToDoList.Validators
{
    public class LoginValidator: AbstractValidator<LogInModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
