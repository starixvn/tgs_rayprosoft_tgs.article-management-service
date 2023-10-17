using System.Text.Json.Serialization;

namespace Tgs.ArticleManagement.Clients;
public record ArticleListItem(
	[property: JsonPropertyName("id")] Guid FormId,
	string Description,
	DateTime UpdatedOn,
	ArticleStatus Status,
	IReadOnlyDictionary<string, Article> LocalizedArticles);
