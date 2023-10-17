namespace Tgs.ArticleManagement.Clients;
public record DraftListItem(
	Guid Id,
	string Description,
	string Modifier,
	DateOnly? PublishDate,
	ArticleStatus Status,
	DateTime CreatedOn,
	DateTime UpdatedOn);
