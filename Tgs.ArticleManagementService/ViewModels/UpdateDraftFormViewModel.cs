namespace Tgs.ArticleManagementService.ViewModels;

public class UpdateDraftFormViewModel
{
	internal static readonly UpdateDraftFormViewModel _Empty = new()
	{
		Description = default!,
		LocalizedArticles = new Dictionary<string, ArticleViewModel>().AsReadOnly()
	};

	public required string Description { get; set; }

	public required IReadOnlyDictionary<string, ArticleViewModel> LocalizedArticles { get; set; }
}
