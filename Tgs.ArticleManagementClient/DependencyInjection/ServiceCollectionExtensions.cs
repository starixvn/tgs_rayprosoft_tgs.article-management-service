using Tgs.ArticleManagement.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddArticleManagementClient(
		this IServiceCollection services,
		Action<IHttpClientBuilder> httpClientConfigure)
	{
		httpClientConfigure(services.AddHttpClient<IArticleManagementClient, ArticleManagementClient>());

		return services;
	}
}
