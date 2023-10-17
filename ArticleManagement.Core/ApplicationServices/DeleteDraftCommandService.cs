using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class DeleteDraftCommandService : ICommandService<DeleteDraftCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public DeleteDraftCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask ExecuteAsync(DeleteDraftCommand command, CancellationToken cancellationToken = default)
		=> _tgsRepository.DeleteDraftAsync(command.FormId, cancellationToken);
}
