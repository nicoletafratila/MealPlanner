﻿using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    public class AddCommandValidator : AbstractValidator<AddCommand>
    {
        public AddCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
