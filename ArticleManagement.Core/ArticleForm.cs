namespace Tgs.ArticleManagement;
public record ArticleForm(
	Guid Id,
	Guid BrandId,
	string Description,
	IReadOnlyDictionary<string, Article> LocalizedArticles,
	ArticleStatus Status,
	string Modifier,
	DateTime UpdatedOn);
