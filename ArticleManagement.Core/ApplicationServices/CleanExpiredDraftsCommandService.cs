using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class CleanExpiredDraftsCommandService : ICommandService<CleanExpiredDraftsCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public CleanExpiredDraftsCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask ExecuteAsync(CleanExpiredDraftsCommand command, CancellationToken cancellationToken = default)
		=> _tgsRepository.RemoveExpiredDraftsAsync(command.ExpiredTime, cancellationToken);
}
