using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Job.Queries.Search
{
    public class SearchQueryHandler(IJobRepository repository, IMapper mapper)
        : IRequestHandler<SearchQuery, PagedList<JobModel>>
    {
        private readonly IJobRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<JobModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
                return new([], new Metadata());

            var qp = request.QueryParameters;

            var (entities, totalCount, skip) = await _repository.SearchAsync(
                qp.Filters, qp.Sorting, qp.PageNumber, qp.PageSize, cancellationToken);

            var models = _mapper.Map<IList<JobModel>>(entities) ?? [];
            var metadata = Metadata.Create(qp.PageNumber, qp.PageSize, totalCount);
            for (var i = 0; i < models.Count; i++)
                models[i].Index = skip + i + 1;

            return new PagedList<JobModel>(models, metadata);
        }
    }
}
