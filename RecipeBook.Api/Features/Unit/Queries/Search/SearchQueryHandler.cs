﻿using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.Search
{
    public class SearchQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<UnitModel>>
    {
        public async Task<PagedList<UnitModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            var results = mapper.Map<IList<UnitModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<UnitModel>()).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(request.QueryParameters!.SortString))
                {
                    results = results.AsQueryable().OrderByPropertyName(request.QueryParameters.SortString, request.QueryParameters.SortDirection).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<UnitModel>(new List<UnitModel>(), new Metadata());
        }
    }
}
