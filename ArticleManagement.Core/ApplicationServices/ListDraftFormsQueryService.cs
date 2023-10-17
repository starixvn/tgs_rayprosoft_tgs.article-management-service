using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class ListDraftFormsQueryService : IQueryService<ListDraftFormsQuery, DraftFormListItem>
{
	private readonly ITgsRepository _tgsRepository;

	public ListDraftFormsQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public IAsyncEnumerable<DraftFormListItem> QueryAsync(ListDraftFormsQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.ListDraftFormsAsync(query.BrandId, cancellationToken);
}
