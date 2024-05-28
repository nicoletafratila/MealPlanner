﻿using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public EditProductCategoryModel? Model { get; set; }
    }
}
