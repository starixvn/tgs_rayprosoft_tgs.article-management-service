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

		await _tgsRepository.SetArticleStatusAsync(
			command.FormId,
			ArticleStatus.Removed,
			command.Modifier,
			cancellationToken).ConfigureAwait(false);
	}
}
