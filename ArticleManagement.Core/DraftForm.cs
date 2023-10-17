using System.Text.Json;
using Json.Patch;

namespace Tgs.ArticleManagement;
public record DraftForm(
	Guid Id,
	Guid BrandId,
	string Description,
	IReadOnlyDictionary<string, Article> LocalizedArticles,
	string Modifier,
	ArticleStatus Status,
	DateOnly? PublishDate,
	DateTime CreatedOn,
	DateTime UpdatedOn)
{
	public static readonly DraftForm Empty = new(
		Guid.Empty,
		Guid.Empty,
		default!,
		new Dictionary<string, Article>().AsReadOnly(),
		default!,
		ArticleStatus.None,
		null,
		DateTime.MinValue,
		DateTime.MinValue);

	public static JsonPatch CreatePatch(DraftForm target, JsonSerializerOptions? serializerOptions = default)
		=> (Empty with
		{
			Id = target.Id,
			BrandId = target.BrandId
		}).CreatePatch(target, serializerOptions);
}
