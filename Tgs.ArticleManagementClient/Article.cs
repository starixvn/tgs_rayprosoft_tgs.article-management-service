using Tgs.ArticleManagement;
namespace Tgs.ArticleManagement.Clients;
public record Article(
	string Title,
	string Brief,
	string Content,
	string Writer,
	IReadOnlyDictionary<string, Uri> Banners);
