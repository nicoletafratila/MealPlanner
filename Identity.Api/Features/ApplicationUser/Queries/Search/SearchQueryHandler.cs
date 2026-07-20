using AutoMapper;
using Common.Pagination;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Queries.Search
{
    public class SearchQueryHandler(
        UserManager<Data.Entities.ApplicationUser> userManager,
        IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ApplicationUserModel>>
    {
        private readonly UserManager<Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public Task<PagedList<ApplicationUserModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
                return Task.FromResult(new PagedList<ApplicationUserModel>([], new Metadata()));

            var qp = request.QueryParameters;

            var users = _userManager.Users
                .OrderBy(u => u.UserName)
                .ToList();

            var models = _mapper.Map<IList<ApplicationUserModel>>(users) ?? [];

            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return Task.FromResult(models.ToPagedList(qp.PageNumber, qp.PageSize));
        }

        private static IList<ApplicationUserModel> ApplyFilters(
            IList<ApplicationUserModel> source,
            QueryParameters<ApplicationUserModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;
            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<ApplicationUserModel>();
                result = result.Where(predicate).ToList();
            }
            return result;
        }

        private static IList<ApplicationUserModel> ApplySorting(
            IList<ApplicationUserModel> source,
            QueryParameters<ApplicationUserModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            return source.AsQueryable()
                         .ApplySorting(parameters.Sorting)!
                         .ToList();
        }
    }
}
