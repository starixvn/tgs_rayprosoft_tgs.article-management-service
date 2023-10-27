namespace Tgs.ArticleManagement;

public interface ITgsRepository
{
	ValueTask<DraftForm?> GetDraftFormAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask<ArticleForm?> GetArticleFormAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask CreateNewDraftAsync(DraftForm draftForm, CancellationToken cancellationToken = default);

	ValueTask PatchDraftAsync(DraftForm changeData, CancellationToken cancellationToken = default);

	ValueTask DeleteDraftAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask UpsertArticleFormAsync(ArticleForm articleForm, CancellationToken cancellationToken = default);

	ValueTask SetArticleStatusAsync(Guid formId, ArticleStatus status, string modifier, CancellationToken cancellationToken = default);

	IAsyncEnumerable<ArticleForm> ListArticleFormsAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DraftFormListItem> ListDraftFormsAsync(Guid brandId, int skip = 0, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DraftForm> ListWaitingSubmissionDraftsAsync(DateOnly publishDate, CancellationToken cancellationToken = default);

	ValueTask RemoveExpiredDraftsAsync(DateTime expriedTime, CancellationToken cancellationToken = default);

	ValueTask<int> CountDraftFormsAsync(Guid brandId, CancellationToken cancellationToken = default);

	ValueTask<int> CountPublishArticleAsync(Guid brandId, CancellationToken cancellationToken = default);
}
