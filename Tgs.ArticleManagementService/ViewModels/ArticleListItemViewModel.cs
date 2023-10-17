using Tgs.ArticleManagement;

namespace Tgs.ArticleManagementService.ViewModels;

public class ArticleListItemViewModel
{
	public required Guid Id { get; set; }

	public required string Description { get; set; }

	public required DateTime UpdatedOn { get; set; }

	public required ArticleStatus Status { get; set; }

	public required IReadOnlyDictionary<string, ArticleViewModel> LocalizedArticles { get; set; }
}
