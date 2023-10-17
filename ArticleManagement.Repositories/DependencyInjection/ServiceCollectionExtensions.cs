using ArticleManagement.Repositories;
using ArticleManagement.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Tgs.ArticleManagement;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddArticleManagementRepositories(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> dbConfigure)
		=> services
			.AddDbContext<TgsContext>(dbConfigure)
			.AddScoped<ITgsRepository, TgsRepository>();
}
