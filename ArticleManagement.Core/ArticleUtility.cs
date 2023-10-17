using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.ArticleManagement;

public static class ArticleUtility
{
	public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
	{
		Converters = {
			new JsonStringEnumConverter()
		}
	};
}
