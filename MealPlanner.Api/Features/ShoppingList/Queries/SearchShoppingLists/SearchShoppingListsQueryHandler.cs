﻿using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.SearchShoppingLists
{
    public class SearchShoppingListsQueryHandler : IRequestHandler<SearchShoppingListsQuery, PagedList<ShoppingListModel>>
    {
        private readonly IShoppingListRepository _repository;
        private readonly IMapper _mapper;

        public SearchShoppingListsQueryHandler(IShoppingListRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedList<ShoppingListModel>> Handle(SearchShoppingListsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<ShoppingListModel>>(data).OrderBy(r => r.Name).ToList();
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
