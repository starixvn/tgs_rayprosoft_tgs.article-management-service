using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class GetDraftFormQueryService : IGetService<GetDraftFormQuery, DraftForm?>
{
	private readonly ITgsRepository _tgsRepository;

	public GetDraftFormQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask<DraftForm?> GetAsync(GetDraftFormQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.GetDraftFormAsync(query.FormId, cancellationToken);
}
