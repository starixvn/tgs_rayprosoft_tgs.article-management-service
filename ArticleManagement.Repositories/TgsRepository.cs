using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using ArticleManagement.Repositories.Entities;
using Json.Patch;
using Json.Pointer;
using Microsoft.EntityFrameworkCore;
using Tgs.ArticleManagement;

namespace ArticleManagement.Repositories;

internal class TgsRepository : ITgsRepository
{
	private readonly TgsContext _dbContext;

	public TgsRepository(TgsContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	public async ValueTask CreateNewDraftAsync(DraftForm draftForm, CancellationToken cancellationToken = default)
	{
		_ = await _dbContext.DraftRows.AddAsync(
			new DraftRow
			{
				Id = draftForm.Id,
				BrandId = draftForm.BrandId,
				UpdatedOn = draftForm.UpdatedOn.ToUniversalTime(),
				CreatedOn = draftForm.CreatedOn.ToUniversalTime(),
				Description = draftForm.Description,
				LocalizedArticles = JsonSerializer.Serialize(draftForm.LocalizedArticles, ArticleUtility.SerializerOptions),
				Modifier = draftForm.Modifier,
				PublishDate = draftForm.PublishDate,
				Status = draftForm.Status.ToString()
			},
			cancellationToken).ConfigureAwait(false);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask DeleteDraftAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var row = await _dbContext.DraftRows.FindAsync(
			new object?[] { formId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		if (row is not null)
			_ = _dbContext.DraftRows.Remove(row);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<ArticleForm?> GetArticleFormAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var row = await _dbContext.ArticleRows.FindAsync(
			new object?[] { formId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return row is null
			? null
			: new ArticleForm(
				row.Id,
				row.BrandId,
				row.Description,
				JsonSerializer.Deserialize<IReadOnlyDictionary<string, Article>>(
					row.LocalizedArticles,
					ArticleUtility.SerializerOptions)!,
				Enum.Parse<ArticleStatus>(row.Status),
				row.Modifier,
				row.UpdatedOn);
	}

	public async ValueTask<DraftForm?> GetDraftFormAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var row = await _dbContext.DraftRows.FindAsync(
			new object?[] { formId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return row is null
			? null
			: new DraftForm(
				row.Id,
				row.BrandId,
				row.Description,
				JsonSerializer.Deserialize<IReadOnlyDictionary<string, Article>>(
					row.LocalizedArticles,
					ArticleUtility.SerializerOptions)!,
				row.Modifier,
				Enum.Parse<ArticleStatus>(row.Status),
				row.PublishDate,
				row.CreatedOn,
				row.UpdatedOn);
	}

	public IAsyncEnumerable<ArticleForm> ListArticleFormsAsync(Guid brandId, CancellationToken cancellationToken = default)
		=> _dbContext.ArticleRows
			.Where(row => row.BrandId == brandId
				&& row.Status == nameof(ArticleStatus.Submission))
			.OrderByDescending(row => row.UpdatedOn)
			.ToAsyncEnumerable()
			.Select(row => new ArticleForm(
				row.Id,
				row.BrandId,
				row.Description,
				JsonSerializer.Deserialize<IReadOnlyDictionary<string, Article>>(
					row.LocalizedArticles,
					ArticleUtility.SerializerOptions)!,
				Enum.Parse<ArticleStatus>(row.Status),
				row.Modifier,
				row.UpdatedOn));

	public IAsyncEnumerable<DraftFormListItem> ListDraftFormsAsync(Guid brandId, CancellationToken cancellationToken = default)
		=> _dbContext.Set<DraftFormViewRow>()
			.FromSqlInterpolated($$"""
				SELECT
					COALESCE(D.id, A.id) AS "id",
					COALESCE(D.brand_id, A.brand_id) as "brand_id",
					COALESCE(D.description, A.description) AS "description",
					COALESCE(D.status, A.status) AS "status",
					COALESCE(D.modifier, A.modifier) AS "modifier",
					D.publish_date,
					COALESCE(D.created_on, A.updated_on) AS "created_on",
					COALESCE(D.updated_on, A.updated_on) AS "updated_on"
				FROM
					dbo.draft_forms AS D
					FULL JOIN
					dbo.article_forms AS A ON D.id = A.id
				WHERE
					D.brand_id = {{brandId}}
					AND
					A.brand_id = {{brandId}}
				""")
			.OrderByDescending(row => row.UpdatedOn)
			.Take(100)
			.ToAsyncEnumerable()
			.Select(row => new DraftFormListItem(
				row.Id,
				row.BrandId,
				row.Description,
				row.Modifier,
				Enum.Parse<ArticleStatus>(row.Status),
				row.PublishDate,
				row.CreatedOn,
				row.UpdatedOn));

	public async ValueTask PatchDraftAsync(DraftForm changeData, CancellationToken cancellationToken = default)
	{
		var patch = (DraftForm.Empty with
		{
			Id = changeData.Id,
			BrandId = changeData.BrandId,
			LocalizedArticles = changeData.LocalizedArticles
		}).CreatePatch(changeData, ArticleUtility.SerializerOptions);

		var row = await _dbContext.DraftRows.FindAsync(
			new object?[] { changeData.Id },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		if (row is null)
			return;

		foreach (var operation in patch.Operations)
		{
			switch (operation.Op)
			{
				case OperationType.Replace:
					ReplaceDraftRowPropertyValue(row, operation.Path, operation.Value!);
					break;
			}
		}

		row.LocalizedArticles = JsonSerializer.Serialize(
			changeData.LocalizedArticles,
			ArticleUtility.SerializerOptions);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask SetArticleStatusAsync(Guid formId, ArticleStatus status, CancellationToken cancellationToken = default)
	{
		var row = await _dbContext.ArticleRows.FindAsync(
			new object?[] { formId },
			cancellationToken: cancellationToken).ConfigureAwait(false);

		if (row is null)
			return;

		row.Status = status.ToString();

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpsertArticleFormAsync(ArticleForm articleForm, CancellationToken cancellationToken = default)
	{
		var row = (await _dbContext.ArticleRows.FindAsync(
			new object?[] { articleForm.Id },
			cancellationToken: cancellationToken).ConfigureAwait(false))
			?? (await _dbContext.AddAsync(
				new ArticleRow
				{
					Id = articleForm.Id,
					BrandId = articleForm.BrandId
				},
				cancellationToken).ConfigureAwait(false)).Entity;

		row.Modifier = articleForm.Modifier;
		row.UpdatedOn = articleForm.UpdatedOn;
		row.Modifier = articleForm.Modifier;
		row.Status = articleForm.Status.ToString();
		row.LocalizedArticles = JsonSerializer.Serialize(articleForm.LocalizedArticles, ArticleUtility.SerializerOptions);
		row.Description = articleForm.Description;

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	private static Expression MakeMemberAccess(object dataRow, JsonPointer path)
	{
		Expression expression = Expression.Constant(dataRow);

		foreach (var segment in path.Segments)
			foreach (var pi in expression.Type.GetProperties())
			{
				var jsonPropertyName = ArticleUtility.SerializerOptions.PropertyNamingPolicy?.ConvertName(pi.Name) ?? pi.Name;

				if (jsonPropertyName == segment.Value)
				{
					expression = Expression.MakeMemberAccess(expression, pi);

					break;
				}
			}

		return expression;
	}

	private static void ReplaceDraftRowPropertyValue<TEntity>(TEntity dataRow, JsonPointer path, JsonNode value)
	{
		var memberAccessExpression = MakeMemberAccess(dataRow ?? throw new NullReferenceException(nameof(dataRow)), path);

		if (memberAccessExpression.Type.IsGenericType && memberAccessExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			var deserializedValue = value.Deserialize(
				memberAccessExpression.Type.GetGenericArguments()[0],
				ArticleUtility.SerializerOptions);

			var assignExpression = Expression.Assign(
				memberAccessExpression,
				Expression.Convert(Expression.Constant(deserializedValue), memberAccessExpression.Type));
			_ = Expression.Lambda(assignExpression).Compile().DynamicInvoke();
		}
		else
		{
			var deserializedValue = value.Deserialize(
				memberAccessExpression.Type,
				ArticleUtility.SerializerOptions);

			var assignExpression = Expression.Assign(
				memberAccessExpression,
				Expression.Constant(deserializedValue));
			_ = Expression.Lambda(assignExpression).Compile().DynamicInvoke();
		}
	}
}
