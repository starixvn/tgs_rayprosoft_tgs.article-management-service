using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class UpdateDraftCommandService : ICommandService<UpdateDraftCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public UpdateDraftCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(UpdateDraftCommand command, CancellationToken cancellationToken = default)
	{
		var draftForm = await _tgsRepository.GetDraftFormAsync(
			command.UpdateData.Id,
			cancellationToken).ConfigureAwait(false);

		if (draftForm is null)
		{
			var articleForm = await _tgsRepository.GetArticleFormAsync(
				command.UpdateData.Id,
				cancellationToken).ConfigureAwait(false);

			if (articleForm is null)
				return;

			var snapshotForm = new DraftForm(
				articleForm.Id,
				articleForm.BrandId,
				articleForm.Description,
				articleForm.LocalizedArticles,
				articleForm.Modifier,
				articleForm.Status,
				null,
				articleForm.UpdatedOn,
				DateTime.UtcNow);

			await _tgsRepository.CreateNewDraftAsync(
				snapshotForm,
				cancellationToken).ConfigureAwait(false);
		}

		await _tgsRepository.PatchDraftAsync(
			command.UpdateData,
			cancellationToken).ConfigureAwait(false);
	}
}
