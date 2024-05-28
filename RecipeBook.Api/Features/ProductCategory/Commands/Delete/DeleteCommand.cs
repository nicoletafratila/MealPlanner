﻿using MediatR;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
