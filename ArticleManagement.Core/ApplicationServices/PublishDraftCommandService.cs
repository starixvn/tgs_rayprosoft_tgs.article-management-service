using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class PublishDraftCommandService : ICommandService<PublishDraftCommand>
{
	private readonly ITgsRepository _tgsRepository;

	public PublishDraftCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(PublishDraftCommand command, CancellationToken cancellationToken = default)
	{
		var form = await _tgsRepository.GetDraftFormAsync(
			command.FormId,
			cancellationToken).ConfigureAwait(false);

		if (form is null)
			return;

		var newArticle = new ArticleForm(
			form.Id,
			form.BrandId,
			form.Description,
			form.LocalizedArticles,
			ArticleStatus.Submission,
			command.Modifier,
			DateTime.UtcNow);

		await _tgsRepository.UpsertArticleFormAsync(
			newArticle,
			cancellationToken).ConfigureAwait(false);
	}
}
