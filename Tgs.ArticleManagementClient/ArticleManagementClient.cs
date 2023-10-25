using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Patch;

namespace Tgs.ArticleManagement.Clients;

internal class ArticleManagementClient : IArticleManagementClient
{
	private static readonly JsonSerializerOptions _SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = {
			new JsonStringEnumConverter()
		}
	};

	private readonly HttpClient _httpClient;

	public ArticleManagementClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public ValueTask<Guid> CreateNewDraftAsync(
		Guid brandId,
		NewDraftForm newForm,
		CancellationToken cancellationToken = default)
		=> _httpClient.PutAsJsonAsync(
			$"/draft/{brandId}",
			newForm,
			_SerializerOptions,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<Guid>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable())
			.FirstAsync(cancellationToken);

	public async ValueTask DeleteDraftAsync(
		Guid formId,
		CancellationToken cancellationToken = default)
		=> _ = await _httpClient.DeleteAsync($"/draft/{formId}", cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.ToListAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	public ValueTask<DraftForm?> GetDraftFormAsync(
		Guid formId,
		CancellationToken cancellationToken = default)
		=> _httpClient.GetAsync($"/draft/detail/{formId}", cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<DraftForm>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable())
			.FirstOrDefaultAsync(cancellationToken);

	public async ValueTask<QueryResult<DraftListItem>> ListDraftFormsAsync(
		Guid brandId,
		int skip = 0,
		CancellationToken cancellationToken = default)
	{
		var totalCount = await CountDraftFormsAsync(
			brandId,
			cancellationToken).ConfigureAwait(false);

		var remainingCount = totalCount - skip;
		var pages = (int)Math.Ceiling(remainingCount / 100D);

		return new QueryResult<DraftListItem>(
			totalCount,
			AsyncEnumerable.Range(0, pages)
			.SelectMany(idx => _httpClient.GetAsync(
				$"/draft/{brandId}?skip={skip + idx * 100}",
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken)
				.ToAsyncEnumerable()
				.Do(response => response.EnsureSuccessStatusCode())
				.SelectMany(response => response.Content
					.ReadFromJsonAsync<IAsyncEnumerable<DraftListItem>>(_SerializerOptions, cancellationToken)
					.ToAsyncEnumerable()
					.Where(items => items is not null)
					.SelectMany(items => items!))));
	}

	public IAsyncEnumerable<ArticleListItem> ListArticleFormsAsync(
		Guid brandId,
		CancellationToken cancellationToken = default)
		=> _httpClient.GetAsync(
			$"/article/{brandId}",
			HttpCompletionOption.ResponseHeadersRead,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<IAsyncEnumerable<ArticleListItem>>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(items => items is not null)
				.SelectMany(items => items!));

	public async ValueTask PublishDraftAsync(
		Guid fromId,
		DateOnly publishDate,
		CancellationToken cancellationToken = default)
		=> _ = await _httpClient.PostAsJsonAsync(
			$"/draft/{fromId}:publish",
			publishDate,
			cancellationToken: cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.ToListAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask UpdateDraftAsync(
		Guid formId,
		UpdateDraftForm updateData,
		CancellationToken cancellationToken = default)
	{
		var patch = UpdateDraftForm.Empty.CreatePatch(updateData, _SerializerOptions);

		_ = await _httpClient.PatchAsync(
			$"/draft/{formId}",
			JsonContent.Create(patch),
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task RemoveArticleAsync(
		Guid formId,
		CancellationToken cancellationToken = default)
		=> _ = await _httpClient.DeleteAsync(
			$"/article/{formId}",
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.ToListAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	internal ValueTask<int> CountDraftFormsAsync(
		Guid brandId,
		CancellationToken cancellationToken = default)
		=> _httpClient.SendAsync(
			new HttpRequestMessage(HttpMethod.Head, $"/draft/{brandId}"),
			HttpCompletionOption.ResponseHeadersRead,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Headers
				.GetValues("Count")
				.Take(1)
				.Select(value => int.Parse(value))
				.ToAsyncEnumerable())
			.FirstAsync(cancellationToken);

	internal async ValueTask DoPublishDraftsAsync(DateOnly? processDate, CancellationToken cancellationToken = default)
		=> _ = await AsyncEnumerableEx.Return(processDate is null
			? "/schedule/publish"
			: $"/schedule/publish/{processDate.Value}")
			.SelectMany(path => _httpClient.PostAsync(path, null, cancellationToken)
				.ToAsyncEnumerable())
			.Do(response => response.EnsureSuccessStatusCode())
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);

	internal async ValueTask DoRemoveExpiredDraftsAsync(CancellationToken cancellationToken = default)
		=> _ = await _httpClient.PostAsync("/schedule/expired-draft", null, cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.ToListAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);
}
