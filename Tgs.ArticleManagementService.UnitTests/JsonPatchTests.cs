using System.Text.Json;
using ExpectedObjects;
using Json.Patch;
using Tgs.ArticleManagement;
using Tgs.ArticleManagementService.Models;
using Tgs.ArticleManagementService.ViewModels;

namespace Tgs.ArticleManagementService.UnitTests;

public class JsonPatchTests
{
	[Fact]
	public void UpdateDraftFormViewModel的JsonPatch轉換成DraftForm()
	{
		// Arrange
		var source = new UpdateDraftFormViewModel
		{
			Description = "test",
			LocalizedArticles = new Dictionary<string, ArticleViewModel>
			{
				["en-US"] = new ArticleViewModel
				{
					Title = "title",
					Brief = "brief",
					Content = "content",
					Writer = "writer",
					Banners = new Dictionary<string, Uri>
					{
						["100"] = new Uri("http://localhost/100.jpg")
					}
				}
			}
		};
		var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		var patch = UpdateDraftFormViewModel._Empty.CreatePatch(source, serializerOptions);

		var draftFormSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
		{
			Converters =
			{
				new UpdateDraftFormViewModelToDraftFormJsonConverter(
					new JsonSerializerOptions(JsonSerializerDefaults.Web))
			}
		};

		var expected = DraftForm.Empty with
		{
			Description = source.Description,
			LocalizedArticles = source.LocalizedArticles
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

		// Act
		var actual = patch.Apply(DraftForm.Empty, draftFormSerializerOptions);

		// Assert
		expected.ToExpectedObject(ctx => ctx
			.IgnoreEqualsOverride()).ShouldEqual(actual);
	}
}
