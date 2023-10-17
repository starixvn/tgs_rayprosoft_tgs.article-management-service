using Microsoft.AspNetCore.Mvc;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;
using Tgs.ArticleManagementService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.ArticleManagementService.Controllers;

[Route("[controller]")]
[ApiController]
public class ArticleController : ControllerBase
{
	[HttpGet("{brandId:Guid}", Name = "ListArticleForms")]
	public IAsyncEnumerable<ArticleListItemViewModel> ListArticleFormsAsync(
		[FromServices] IQueryService<ListArticleFormsQuery, ArticleForm> queryService,
		Guid brandId)
	{
		var query = new ListArticleFormsQuery(brandId);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(form => new ArticleListItemViewModel
			{
				Id = form.Id,
				Description = form.Description,
				Status = form.Status,
				UpdatedOn = form.UpdatedOn,
				LocalizedArticles = form.LocalizedArticles
					.ToDictionary(
						kv => kv.Key,
						kv => new ArticleViewModel
						{
							Title = kv.Value.Title,
							Brief = kv.Value.Brief,
							Content = kv.Value.Content,
							Writer = kv.Value.Writer,
							Banners = kv.Value.Banners
						})
			});
	}

	[HttpDelete("{formId:Guid}")]
	public ValueTask RemoveArticleAsync(
		[FromServices] ICommandService<RemoveArticleCommand> removeArticleService,
		[FromHeader(Name = "Modifier")] string modifier,
		Guid formId)
	{
		var command = new RemoveArticleCommand(formId, modifier);

		return removeArticleService.ExecuteAsync(command, HttpContext.RequestAborted);
	}
}
