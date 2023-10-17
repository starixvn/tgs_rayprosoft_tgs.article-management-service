using Tgs.ServiceUtility;

namespace Tgs.ArticleManagement.ApplicationServices;

internal class CreateNewDraftCommandService : IGetService<CreateNewDraftCommand, Guid>
{
	private readonly ITgsRepository _tgsRepository;

	public CreateNewDraftCommandService(ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask<Guid> GetAsync(CreateNewDraftCommand command, CancellationToken cancellationToken = default)
	{
		var currentDate = DateTime.Now;

		var newForm = new DraftForm(
			Guid.NewGuid(),
			command.BrandId,
			command.Description,
			command.LocalizedArticles,
			command.Modifier,
			ArticleStatus.Draft,
			command.PublishDate,
			currentDate,
			currentDate);

		await _tgsRepository.CreateNewDraftAsync(
			newForm,
			cancellationToken).ConfigureAwait(false);

		return newForm.Id;
	}
}
