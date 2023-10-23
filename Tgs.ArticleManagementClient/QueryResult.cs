namespace Tgs.ArticleManagement.Clients;
public record QueryResult<TItemType>(
	int TotalCount,
	IAsyncEnumerable<TItemType> Items);
