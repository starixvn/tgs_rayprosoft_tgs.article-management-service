namespace Tgs.ArticleManagementService.ViewModels;

public class CreateDraftFormViewModel
{
	public required string Description { get; set; }

	public DateOnly? PublishDate { get; set; }

	public required IReadOnlyDictionary<string, ArticleViewModel> LocalizedArticles { get; set; }
}
