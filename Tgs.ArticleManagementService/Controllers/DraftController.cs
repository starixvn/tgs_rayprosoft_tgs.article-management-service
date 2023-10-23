using System.Text.Json;
using Json.Patch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;
using Tgs.ArticleManagementService.Models;
using Tgs.ArticleManagementService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.ArticleManagementService.Controllers;

[Route("[controller]")]
[ApiController]
public class DraftController : ControllerBase
{
	[HttpPut("{brandId:Guid}")]
	public ValueTask<Guid> CreateNewDraftAsync(
		[FromServices] IGetService<CreateNewDraftCommand, Guid> createDraftService,
		[FromHeader(Name = "Modifier")] string modifier,
		Guid brandId,
		[FromBody] CreateDraftFormViewModel viewModel)
	{
		var command = new CreateNewDraftCommand(
			brandId,
			viewModel.Description,
			viewModel.PublishDate,
			viewModel.LocalizedArticles
				.ToDictionary(
					kv => kv.Key,
					kv => new Article(
						kv.Value.Title,
						kv.Value.Brief,
						kv.Value.Content,
						kv.Value.Writer,
						kv.Value.Banners)),
			modifier);

		return createDraftService.GetAsync(command, HttpContext.RequestAborted);
	}

	[HttpPost("{formId:Guid}:publish")]
	public ValueTask SetPublishDateAsync(
		[FromServices] ICommandService<SetPublishDateCommand> setService,
		[FromHeader(Name = "Modifier")] string modifier,
		Guid formId,
		[FromBody] DateOnly publishDate)
	{
		var command = new SetPublishDateCommand(formId, publishDate, modifier);

		return setService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPatch("{formId:Guid}")]
	public async ValueTask UpdateDraftAsync(
		[FromServices] ICommandService<UpdateDraftCommand> updateService,
		[FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
		[FromHeader(Name = "Modifier")] string modifier,
		Guid formId,
		[FromBody] JsonPatch patchModel)
	{
		var webSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;

		var patchedData = patchModel.Apply(
			UpdateDraftFormViewModel._Empty,
			webSerializerOptions);

		if (patchedData is null)
			return;

		var updateDraftForm = UpdateDraftFormViewModel._Empty
			.CreatePatch(patchedData, webSerializerOptions)
			.Apply(
				DraftForm.Empty,
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
				{
					Converters = {
						new UpdateDraftFormViewModelToDraftFormJsonConverter(webSerializerOptions)
					}
				});

		if (updateDraftForm is null)
		{
			HttpContext.TransformResult(Problem(title: "PatchDataParseError", statusCode: 400));
			return;
		}

		var command = new UpdateDraftCommand(updateDraftForm with
		{
			Id = formId,
			Modifier = modifier
		});

		await updateService.ExecuteAsync(
			command,
			HttpContext.RequestAborted).ConfigureAwait(false);
	}

	[HttpDelete("{formId:Guid}")]
	public ValueTask DeleteDraftAsync(
		[FromServices] ICommandService<DeleteDraftCommand> deleteService,
		Guid formId)
	{
		var command = new DeleteDraftCommand(formId);

		return deleteService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpGet("{brandId:Guid}", Name = "ListDraftForms")]
	public IAsyncEnumerable<DraftListItemViewModel> ListDraftFormsAsync(
		[FromServices] IQueryService<ListDraftFormsQuery, DraftFormListItem> queryService,
		Guid brandId,
		[FromQuery(Name = "skip")] int skip = 0)
	{
		var query = new ListDraftFormsQuery(brandId, skip);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(item => new DraftListItemViewModel
			{
				Id = item.Id,
				Description = item.Description,
				Status = item.Status,
				Modifier = item.Modifier,
				CreatedOn = item.CreatedOn,
				UpdatedOn = item.UpdatedOn,
				PublishDate = item.PublishDate
			});
	}

	[HttpGet("detail/{formId:Guid}", Name = "GetDraftForm")]
	public async Task<DraftFormViewModel?> GetDraftFormAsync(
		[FromServices] IGetService<GetDraftFormQuery, DraftForm?> getService,
		Guid formId)
	{
		var query = new GetDraftFormQuery(formId);

		var form = await getService.GetAsync(
			query,
			HttpContext.RequestAborted).ConfigureAwait(false);

		return form is null
			? null
			: new DraftFormViewModel
			{
				Id = form.Id,
				BrandId = form.BrandId,
				Description = form.Description,
				Status = form.Status,
				CreatedOn = form.CreatedOn,
				UpdatedOn = form.UpdatedOn,
				Modifier = form.Modifier,
				PublishDate = form.PublishDate,
				LocalizedArticles = form.LocalizedArticles
					.ToDictionary(
						kv => kv.Key,
						kv => new ArticleViewModel
						{
							Title = kv.Value.Title,
							Brief = kv.Value.Brief,
							Banners = kv.Value.Banners,
							Content = kv.Value.Content,
							Writer = kv.Value.Writer
						})
			};
	}
}
