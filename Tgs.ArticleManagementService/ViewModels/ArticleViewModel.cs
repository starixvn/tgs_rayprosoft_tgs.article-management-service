namespace Tgs.ArticleManagementService.ViewModels;

public class ArticleViewModel
{
	public required string Title { get; set; }

	public required string Brief { get; set; }

	public required string Content { get; set; }

	public required string Writer { get; set; }

	public required IReadOnlyDictionary<string, Uri> Banners { get; set; }
}
