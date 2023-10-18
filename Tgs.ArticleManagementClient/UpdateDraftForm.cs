namespace Tgs.ArticleManagement.Clients;
public record UpdateDraftForm(
	string Description,
	IReadOnlyDictionary<string, Article> LocalizedArticles)
{
	public static readonly UpdateDraftForm Empty = new(
		default!,
		new Dictionary<string, Article>());
}
