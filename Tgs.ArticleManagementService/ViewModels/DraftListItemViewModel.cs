using Tgs.ArticleManagement;

namespace Tgs.ArticleManagementService.ViewModels;

public class DraftListItemViewModel
{
	public required Guid Id { get; set; }

	public required string Description { get; set; }

	public required string Modifier { get; set; }

	public DateOnly? PublishDate { get; set; }

	public required ArticleStatus Status { get; set; }

	public required DateTime CreatedOn { get; set; }

	public required DateTime UpdatedOn { get; set; }
}
