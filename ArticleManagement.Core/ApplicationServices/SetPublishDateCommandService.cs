using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class SetPublishDateCommandService : ICommandService<SetPublishDateCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public SetPublishDateCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(SetPublishDateCommand command, CancellationToken cancellationToken = default)
	{
		var draftForm = await _tgsRepository.GetDraftFormAsync(
			command.FormId,
			cancellationToken).ConfigureAwait(false);

		if (draftForm is null)
			return;

		var publishedCount = await _tgsRepository.CountPublishArticleAsync(
			draftForm.BrandId,
			cancellationToken).ConfigureAwait(false);

		if (publishedCount + 1 > 20)
			throw new PublishCountExceedLimitException();

		var modifyData = DraftForm.Empty with
		{
			Id = command.FormId,
			PublishDate = command.PublishDate,
			Status = ArticleStatus.WaitingSubmission,
			Modifier = command.Modifier
		};

		await _tgsRepository.PatchDraftAsync(
			modifyData,
			cancellationToken).ConfigureAwait(false);
	}
}
