namespace Tgs.ArticleManagement.Clients;

public interface IArticleManagementClient
{
	ValueTask<Guid> CreateNewDraftAsync(Guid brandId, NewDraftForm newForm, CancellationToken cancellationToken = default);

	ValueTask DeleteDraftAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask<DraftForm?> GetDraftFormAsync(Guid formId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<ArticleListItem> ListArticleFormsAsync(Guid brandId, CancellationToken cancellationToken = default);

	IAsyncEnumerable<DraftListItem> ListDraftFormsAsync(Guid brandId, int skip = 0, CancellationToken cancellationToken = default);

	ValueTask PublishDraftAsync(Guid fromId, DateOnly publishDate, CancellationToken cancellationToken = default);

	Task RemoveArticleAsync(Guid formId, CancellationToken cancellationToken = default);

	ValueTask UpdateDraftAsync(Guid formId, UpdateDraftForm updateData, CancellationToken cancellationToken = default);
}
