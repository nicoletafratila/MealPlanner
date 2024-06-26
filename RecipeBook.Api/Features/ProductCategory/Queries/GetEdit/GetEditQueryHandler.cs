﻿using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{
    public class GetEditQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ProductCategoryEditModel>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductCategoryEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<ProductCategoryEditModel>(result);
        }
    }
}
