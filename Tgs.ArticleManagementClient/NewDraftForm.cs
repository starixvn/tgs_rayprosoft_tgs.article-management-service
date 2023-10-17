using Tgs.ArticleManagement.Clients;
using Tgs.ArticleManagement;

namespace Tgs.ArticleManagement.Clients;
public record NewDraftForm(
	string Description,
	DateOnly? PublishDate,
	IReadOnlyDictionary<string, Article> LocalizedArticles);
