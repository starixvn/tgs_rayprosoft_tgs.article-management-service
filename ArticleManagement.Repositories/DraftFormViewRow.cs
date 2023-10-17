namespace ArticleManagement.Repositories;

public class DraftFormViewRow
{
	public Guid Id { get; set; }

	public Guid BrandId { get; set; }

	public string Description { get; set; } = null!;

	public string Status { get; set; } = null!;

	public string Modifier { get; set; } = null!;

	public DateOnly? PublishDate { get; set; }

	public DateTime CreatedOn { get; set; }

	public DateTime UpdatedOn { get; set; }
}
