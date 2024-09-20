﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using BE_EXE201.Dtos;

namespace BE_EXE201.Validation
{
    public class UserValidator : AbstractValidator<UserModel>
    {
        public UserValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Wrong Email Format");          
        }
    }
    public static class UserValidatorExtension

    {
        public static async Task<ValidationProblemDetails> ValidateAsync(this UserModel user)
        {
            var validator = new UserValidator();
            var result = await validator.ValidateAsync(user);
            if (!result.IsValid) 
            {
                return result.ToProblemDetails();
            }
            return null;
        }
    }
}