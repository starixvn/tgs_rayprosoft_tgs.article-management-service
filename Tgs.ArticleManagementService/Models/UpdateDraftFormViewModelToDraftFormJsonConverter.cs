using System.Text.Json;
using System.Text.Json.Serialization;
using Tgs.ArticleManagement;
using Tgs.ArticleManagementService.ViewModels;

namespace Tgs.ArticleManagementService.Models;

internal class UpdateDraftFormViewModelToDraftFormJsonConverter : JsonConverter<DraftForm>
{
	private readonly JsonSerializerOptions _sourceSerializerOptions;

	public UpdateDraftFormViewModelToDraftFormJsonConverter(JsonSerializerOptions sourceSerializerOptions)
	{
		_sourceSerializerOptions = sourceSerializerOptions ?? throw new ArgumentNullException(nameof(sourceSerializerOptions));
	}

	public override DraftForm? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var viewModel = JsonSerializer.Deserialize<UpdateDraftFormViewModel>(ref reader, _sourceSerializerOptions);

		return viewModel is null
			? null
			: DraftForm.Empty with
			{
				Description = viewModel.Description,
				LocalizedArticles = viewModel.LocalizedArticles
					.ToDictionary(
						kv => kv.Key,
						kv => new Article(
							kv.Value.Title,
							kv.Value.Brief,
							kv.Value.Content,
							kv.Value.Writer,
							kv.Value.Banners))
					.AsReadOnly()
			};
	}

	public override void Write(Utf8JsonWriter writer, DraftForm value, JsonSerializerOptions options)
	{
		var viewModel = new UpdateDraftFormViewModel
		{
			Description = value.Description,
			LocalizedArticles = value.LocalizedArticles
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
				.AsReadOnly()
		};

		JsonSerializer.Serialize(writer, viewModel, _sourceSerializerOptions);
	}
}
