﻿using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    public class SearchQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ProductCategoryModel>>
    {
        public async Task<PagedList<ProductCategoryModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            var results = mapper.Map<IList<ProductCategoryModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<ProductCategoryModel>()).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(request.QueryParameters!.SortString))
                {
                    results = results.AsQueryable().OrderByPropertyName(request.QueryParameters.SortString, request.QueryParameters.SortDirection).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<ProductCategoryModel>(new List<ProductCategoryModel>(), new Metadata());
        }
    }
}
