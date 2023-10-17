using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class SetPublishDateCommandService : ICommandService<SetPublishDateCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public SetPublishDateCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask ExecuteAsync(SetPublishDateCommand command, CancellationToken cancellationToken = default)
	{
		var modifyData = DraftForm.Empty with
		{
			Id = command.FormId,
			PublishDate = command.PublishDate,
			Status = ArticleStatus.WaitingSubmission,
			Modifier = command.Modifier
		};

		return _tgsRepository.PatchDraftAsync(modifyData, cancellationToken);
	}
}
