﻿using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
