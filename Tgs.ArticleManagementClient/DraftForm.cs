namespace Tgs.ArticleManagement.Clients;
public record DraftForm(
	Guid Id,
	Guid BrandId,
	string Description,
	IReadOnlyDictionary<string, Article> LocalizedArticles,
	string Modifier,
	ArticleStatus Status,
	DateOnly? PublishDate,
	DateTime CreatedOn,
	DateTime UpdatedOn);
