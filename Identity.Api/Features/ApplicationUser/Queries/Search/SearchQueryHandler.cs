using AutoMapper;
using Common.Data.Repository;
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

        public async Task<PagedList<ApplicationUserModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
                return new([], new Metadata());

            var qp = request.QueryParameters;

            var filters = (qp.Filters ?? []).ToList();
            var sorting = (qp.Sorting ?? []).ToList();

            var lockedOutFilter = filters.FirstOrDefault(f =>
                string.Equals(f.PropertyName, nameof(ApplicationUserModel.IsLockedOut), StringComparison.OrdinalIgnoreCase));
            var remainingFilters = lockedOutFilter is null ? filters : filters.Where(f => f != lockedOutFilter).ToList();

            var lockedOutSort = sorting.FirstOrDefault(s =>
                string.Equals(s.PropertyName, nameof(ApplicationUserModel.IsLockedOut), StringComparison.OrdinalIgnoreCase));
            var remainingSorting = lockedOutSort is null ? sorting : sorting.Where(s => s != lockedOutSort).ToList();

            IQueryable<Data.Entities.ApplicationUser> query = _userManager.Users;

            if (lockedOutFilter is not null)
                query = ApplyIsLockedOutFilter(query, lockedOutFilter);

            query = query.ApplyFilters(remainingFilters);

            query = lockedOutSort is not null
                ? ApplyIsLockedOutSort(query, lockedOutSort)
                : query.OrderBy(u => u.UserName).ApplySorting(remainingSorting);

            var (entities, totalCount, skip) = await query.ToPagedResultAsync(qp.PageNumber, qp.PageSize, cancellationToken);

            var models = _mapper.Map<IList<ApplicationUserModel>>(entities) ?? [];
            var metadata = Metadata.Create(qp.PageNumber, qp.PageSize, totalCount);
            for (var i = 0; i < models.Count; i++)
                models[i].Index = skip + i + 1;

            return new PagedList<ApplicationUserModel>(models, metadata);
        }

        private static IQueryable<Data.Entities.ApplicationUser> ApplyIsLockedOutFilter(
            IQueryable<Data.Entities.ApplicationUser> query, FilterItem filter)
        {
            var value = Convert.ToBoolean(filter.Value);
            var wantLockedOut = filter.Operator == FilterOperator.NotEquals ? !value : value;
            var now = DateTimeOffset.UtcNow;

            return wantLockedOut
                ? query.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > now)
                : query.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= now);
        }

        private static IQueryable<Data.Entities.ApplicationUser> ApplyIsLockedOutSort(
            IQueryable<Data.Entities.ApplicationUser> query, SortingModel sorting)
        {
            var now = DateTimeOffset.UtcNow;

            return sorting.Direction == SortDirection.Descending
                ? query.OrderByDescending(u => u.LockoutEnd.HasValue && u.LockoutEnd > now)
                : query.OrderBy(u => u.LockoutEnd.HasValue && u.LockoutEnd > now);
        }
    }
}
