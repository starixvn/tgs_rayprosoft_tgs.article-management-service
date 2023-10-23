using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class ListDraftFormsQueryService
	: IQueryService<ListDraftFormsQuery, DraftFormListItem>
	, IGetService<ListDraftFormsQuery, int>
{
	private readonly ITgsRepository _tgsRepository;

	public ListDraftFormsQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public IAsyncEnumerable<DraftFormListItem> QueryAsync(ListDraftFormsQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.ListDraftFormsAsync(query.BrandId, query.Skip, cancellationToken);

	public ValueTask<int> GetAsync(ListDraftFormsQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.CountDraftFormsAsync(query.BrandId, cancellationToken);
}
