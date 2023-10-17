using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class RemoveArticleCommandService : ICommandService<RemoveArticleCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public RemoveArticleCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(RemoveArticleCommand command, CancellationToken cancellationToken = default)
	{
		var article = await _tgsRepository.GetArticleFormAsync(
			command.FormId,
			cancellationToken).ConfigureAwait(false);

		if (article is null)
			return;

		var draft = await _tgsRepository.GetDraftFormAsync(
			command.FormId,
			cancellationToken).ConfigureAwait(false);

		if (draft is null)
			await _tgsRepository.CreateNewDraftAsync(
				new DraftForm(
					article.Id,
					article.BrandId,
					article.Description,
					article.LocalizedArticles,
					command.Modifier,
					ArticleStatus.Removed,
					null,
					article.UpdatedOn,
					DateTime.UtcNow),
				cancellationToken).ConfigureAwait(false);

		await _tgsRepository.SetArticleStatusAsync(
			command.FormId,
			ArticleStatus.Removed,
			cancellationToken).ConfigureAwait(false);
	}
}
