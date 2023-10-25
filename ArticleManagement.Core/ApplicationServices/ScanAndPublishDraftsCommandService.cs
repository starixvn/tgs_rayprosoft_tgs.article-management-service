using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class ScanAndPublishDraftsCommandService : ICommandService<ScanAndPublishDraftsCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public ScanAndPublishDraftsCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(ScanAndPublishDraftsCommand command, CancellationToken cancellationToken = default)
	{
		var waitingSubmissionDrafts = await _tgsRepository.ListWaitingSubmissionDraftsAsync(
			command.ProcessDate,
			cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var draft in waitingSubmissionDrafts)
		{
			var newArticle = new ArticleForm(
				draft.Id,
				draft.BrandId,
				draft.Description,
				draft.LocalizedArticles,
				ArticleStatus.Submission,
				draft.Modifier,
				DateTime.UtcNow);

			await _tgsRepository.UpsertArticleFormAsync(
				newArticle,
				cancellationToken).ConfigureAwait(false);

			await _tgsRepository.DeleteDraftAsync(
				draft.Id,
				cancellationToken).ConfigureAwait(false);
		}
	}
}
