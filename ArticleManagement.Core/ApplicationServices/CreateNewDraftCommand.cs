namespace Tgs.ArticleManagement.ApplicationServices;
public record CreateNewDraftCommand(
	Guid BrandId,
	string Description,
	DateOnly? PublishDate,
	IReadOnlyDictionary<string, Article> LocalizedArticles,
	string Modifier);
