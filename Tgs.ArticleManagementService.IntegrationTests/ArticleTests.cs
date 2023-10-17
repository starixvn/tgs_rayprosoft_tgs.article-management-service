using ArticleManagement.Repositories.Entities;
using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.Clients;
using Xunit.Abstractions;

namespace Tgs.ArticleManagementService.IntegrationTests;

public class ArticleTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ArticleTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 下架文章()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper)));

        var modifier = "tester1";

        var httpClient = web.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Modifier", modifier);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var formId = Guid.NewGuid();

        _ = await dbContext.ArticleRows.AddAsync(
            new ArticleRow
            {
                Id = formId,
                BrandId = Guid.NewGuid(),
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = nameof(ArticleStatus.Submission),
                UpdatedOn = DateTime.Now
            });
        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.RemoveArticleAsync(formId);
        var actualArticleRow = await dbContext.ArticleRows.FindAsync(formId);
        var actualDraftRow = await dbContext.DraftRows.FindAsync(formId);

        // Assert
        Assert.NotNull(actualArticleRow);
        Assert.Equal(nameof(ArticleStatus.Removed), actualArticleRow.Status);
        Assert.NotNull(actualDraftRow);
        Assert.Equal(nameof(ArticleStatus.Removed), actualDraftRow.Status);
        Assert.Equal(modifier, actualDraftRow.Modifier);
    }

    [Fact]
    public async Task 列表文章()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()));

        var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<ArticleManagementClient>());

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var brandId = Guid.NewGuid();

        var rows = new[]
        {
            new ArticleRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = nameof(ArticleStatus.Submission),
                UpdatedOn = DateTime.UtcNow
            },
            new ArticleRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = nameof(ArticleStatus.Submission),
                UpdatedOn = DateTime.UtcNow.AddMinutes(1D)
            },
            new ArticleRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = nameof(ArticleStatus.Removed),
                UpdatedOn = DateTime.UtcNow.AddMinutes(2D)
            }
        };

        await dbContext.ArticleRows.AddRangeAsync(rows);

        _ = await dbContext.DraftRows.AddAsync(new DraftRow
        {
            Id = Guid.NewGuid(),
            BrandId = brandId,
            Description = "desc",
            LocalizedArticles = "{}",
            Modifier = "system",
            Status = nameof(ArticleStatus.Draft),
            UpdatedOn = DateTime.UtcNow
        });

        _ = await dbContext.SaveChangesAsync();

        var expected = rows.Select(row => new ArticleListItem(
            row.Id,
            row.Description,
            row.UpdatedOn,
            Enum.Parse<ArticleStatus>(row.Status),
            new Dictionary<string, ArticleManagement.Clients.Article>().AsReadOnly()))
            .Take(2)
            .ToArray()
            .Reverse();

        // Act
        var actual = await sut.ListArticleFormsAsync(brandId).ToArrayAsync();

        // Assert
        Assert.NotNull(actual);
        expected.ToExpectedObject(ctx => ctx
            .ArrayMemberShouldMatch(
                o => o,
                memberCtx => memberCtx
                    .Ignore(o => o.LocalizedArticles)
                    .IgnoreEqualsOverride())
            .IgnoreEqualsOverride()).ShouldEqual(actual);
    }
}
