namespace Tgs.ArticleManagement;
public record Article(
	string Title,
	string Brief,
	string Content,
	string Writer,
	IReadOnlyDictionary<string, Uri> Banners);
