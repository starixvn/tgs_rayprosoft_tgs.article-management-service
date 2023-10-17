namespace Tgs.ArticleManagement;
public record DraftFormListItem(
	Guid Id,
	Guid BrandId,
	string Description,
	string Modifier,
	ArticleStatus Status,
	DateOnly? PublishDate,
	DateTime CreatedOn,
	DateTime UpdatedOn);
