using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class ListArticleFormsQueryService : IQueryService<ListArticleFormsQuery, ArticleForm>
{
	private readonly ITgsRepository _tgsRepository;

	public ListArticleFormsQueryService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public IAsyncEnumerable<ArticleForm> QueryAsync(ListArticleFormsQuery query, CancellationToken cancellationToken = default)
		=> _tgsRepository.ListArticleFormsAsync(query.BrandId, cancellationToken);
}
