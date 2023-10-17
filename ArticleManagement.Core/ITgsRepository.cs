namespace Tgs.ArticleManagement;

public interface ITgsRepository
{
	ValueTask<DraftForm?> GetDraftFormAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask<ArticleForm?> GetArticleFormAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask CreateNewDraftAsync(DraftForm draftForm, CancellationToken cancellationToken = default);

	ValueTask PatchDraftAsync(DraftForm changeData, CancellationToken cancellationToken = default);

	ValueTask DeleteDraftAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask UpsertArticleFormAsync(ArticleForm articleForm, CancellationToken cancellationToken = default);

	ValueTask SetArticleStatusAsync(Guid formId, ArticleStatus status, CancellationToken cancellationToken = default);

	IAsyncEnumerable<ArticleForm> ListArticleFormsAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DraftFormListItem> ListDraftFormsAsync(Guid brandId, CancellationToken cancellationToken = default);
}
