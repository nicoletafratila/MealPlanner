﻿using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public EditRecipeModel? Model { get; set; }
    }
}
