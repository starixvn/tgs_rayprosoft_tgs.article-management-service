using System.Linq.Expressions;
using System.Text.Json;
using ArticleManagement.Repositories;
using ArticleManagement.Repositories.Entities;
using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.Clients;
using Xunit;
using Xunit.Abstractions;

namespace Tgs.ArticleManagementService.IntegrationTests;

public class DraftTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DraftTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 新增一則文章草稿()
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

        var brandId = Guid.NewGuid();
        var draftForm = new NewDraftForm(
            "testDesc",
            null,
            new Dictionary<string, ArticleManagement.Clients.Article>
            {
                ["en-US"] = new ArticleManagement.Clients.Article(
                    "title",
                    "brief",
                    "content",
                    "writer",
                    new Dictionary<string, Uri>
                    {
                        ["100"] = new Uri("http://localhost/100.jpg")
                    }.AsReadOnly())
            }.AsReadOnly());

        // Act
        var insertedId = await sut.CreateNewDraftAsync(brandId, draftForm);
        var actual = await dbContext.DraftRows.FindAsync(insertedId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(draftForm.Description, actual.Description);
        Assert.Equal(modifier, actual.Modifier);
    }

    [Fact]
    public async Task 更新草稿內容()
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
        var updateData = new UpdateDraftForm(
            "testDesc",
            new Dictionary<string, ArticleManagement.Clients.Article>
            {
                ["en-US"] = new ArticleManagement.Clients.Article(
                    "title",
                    "brief",
                    "content",
                    "writer",
                    new Dictionary<string, Uri>
                    {
                        ["100"] = new Uri("http://localhost/100.jpg")
                    }.AsReadOnly())
            }.AsReadOnly());

        _ = await dbContext.DraftRows.AddAsync(
            new DraftRow
            {
                Id = formId,
                BrandId = Guid.NewGuid(),
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = "Draft",
                UpdatedOn = DateTime.Now
            });

        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.UpdateDraftAsync(formId, updateData);
        var actual = await dbContext.DraftRows.FindAsync(formId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(updateData.Description, actual.Description);
        Assert.Equal(
            JsonSerializer.Serialize(updateData.LocalizedArticles, ArticleUtility.SerializerOptions),
            actual.LocalizedArticles);
        Assert.Equal(modifier, actual.Modifier);
    }

    [Fact]
    public async Task 刪除草稿()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper)));

        var httpClient = web.CreateClient();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var formId = Guid.NewGuid();
        _ = await dbContext.DraftRows.AddAsync(
            new DraftRow
            {
                Id = formId,
                BrandId = Guid.NewGuid(),
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "tester1",
                Status = "Draft",
                UpdatedOn = DateTime.Now
            });

        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.DeleteDraftAsync(formId);
        var actual = await dbContext.DraftRows.FindAsync(formId);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task 將草稿發行成文章()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                        _ = dbContextOptionsBuilder
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(x => x.Ignore(
                                InMemoryEventId.TransactionIgnoredWarning))
                            .ReplaceService<
                                IQueryTranslationPreprocessorFactory,
                                TestInMemoryQueryTranslationPreprocessorFactory<ListDraftFormsQueryTranslationPreprocessor>>();

                        return new TgsContext(dbContextOptionsBuilder.Options);
                    }))));

        var modifier = "tester1";

        var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<ArticleManagementClient>());
        httpClient.DefaultRequestHeaders.Add("Modifier", modifier);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var formId = Guid.NewGuid();
        var publishDate = DateOnly.Parse("2023/10/23");

        _ = await dbContext.DraftRows.AddAsync(
            new DraftRow
            {
                Id = formId,
                BrandId = Guid.NewGuid(),
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = "Draft",
                UpdatedOn = DateTime.Now
            });
        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.PublishDraftAsync(formId, publishDate);
        var actual = await dbContext.DraftRows.FindAsync(formId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(modifier, actual.Modifier);
        Assert.Equal(nameof(ArticleStatus.WaitingSubmission), actual.Status);
        Assert.Equal(publishDate, actual.PublishDate);
    }

    [Fact]
    public async Task 將草稿發行成文章_已經達到最大發行限制篇數()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                        _ = dbContextOptionsBuilder
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(x => x.Ignore(
                                InMemoryEventId.TransactionIgnoredWarning))
                            .ReplaceService<
                                IQueryTranslationPreprocessorFactory,
                                TestInMemoryQueryTranslationPreprocessorFactory<ListDraftFormsQueryTranslationPreprocessor>>();

                        return new TgsContext(dbContextOptionsBuilder.Options);
                    }))));

        var modifier = "tester1";

        var httpClient = web.CreateDefaultClient(web.Services.ResolveLoggingHandler<ArticleManagementClient>());
        httpClient.DefaultRequestHeaders.Add("Modifier", modifier);

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var formId = Guid.NewGuid();
        var publishDate = DateOnly.Parse("2023/10/23");
        var brandId = Guid.NewGuid();

        _ = await dbContext.DraftRows.AddAsync(
            new DraftRow
            {
                Id = formId,
                BrandId = brandId,
                Description = "desc",
                LocalizedArticles = "{}",
                Modifier = "system",
                Status = "Draft",
                UpdatedOn = DateTime.Now
            });

        var currentTime = DateTime.UtcNow;
        var rows = Enumerable.Range(0, 20)
            .Select(n => new DraftFormViewRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                Modifier = "system",
                Status = nameof(ArticleStatus.Submission),
                UpdatedOn = currentTime.AddMinutes(n)
            })
            .ToArray();

        await dbContext.Set<DraftFormViewRow>().AddRangeAsync(rows);

        _ = await dbContext.SaveChangesAsync();

        // Act
        await Assert.ThrowsAsync<ArticleManagement.Clients.PublishCountExceedLimitException>(
            async () => await sut.PublishDraftAsync(formId, publishDate));
        var actual = await dbContext.DraftRows.FindAsync(formId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(nameof(ArticleStatus.Draft), actual.Status);
        Assert.Null(actual.PublishDate);
    }

    [Fact]
    public async Task 取得草稿內容()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()));

        var httpClient = web.CreateClient();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var formId = Guid.NewGuid();

        var row = new DraftRow
        {
            Id = formId,
            BrandId = Guid.NewGuid(),
            Description = "desc",
            LocalizedArticles = "{}",
            Modifier = "system",
            Status = "Draft",
            UpdatedOn = DateTime.Now
        };

        _ = await dbContext.DraftRows.AddAsync(row);

        _ = await dbContext.SaveChangesAsync();

        var expected = new ArticleManagement.Clients.DraftForm(
            formId,
            row.BrandId,
            row.Description,
            new Dictionary<string, ArticleManagement.Clients.Article>(),
            row.Modifier,
            Enum.Parse<ArticleStatus>(row.Status),
            null,
            row.CreatedOn,
            row.UpdatedOn);

        // Act
        var actual = await sut.GetDraftFormAsync(formId);

        // Assert
        Assert.NotNull(actual);
        expected.ToExpectedObject(ctx => ctx
            .IgnoreEqualsOverride()).ShouldEqual(actual);
    }

    [Fact]
    public async Task 列表草稿及文章()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                        _ = dbContextOptionsBuilder
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(x => x.Ignore(
                                InMemoryEventId.TransactionIgnoredWarning))
                            .ReplaceService<
                                IQueryTranslationPreprocessorFactory,
                                TestInMemoryQueryTranslationPreprocessorFactory<ListDraftFormsQueryTranslationPreprocessor>>();

                        return new TgsContext(dbContextOptionsBuilder.Options);
                    }))));

        var httpClient = web.CreateClient();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var brandId = Guid.NewGuid();

        var rows = new[]
        {
            new DraftFormViewRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                Modifier = "system",
                Status = nameof(ArticleStatus.Draft),
                UpdatedOn = DateTime.UtcNow
            },
            new DraftFormViewRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                Modifier = "system",
                Status = nameof(ArticleStatus.Submission),
                UpdatedOn = DateTime.UtcNow.AddMinutes(1D)
            },
            new DraftFormViewRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                Modifier = "system",
                Status = nameof(ArticleStatus.Draft),
                UpdatedOn = DateTime.UtcNow.AddMinutes(2D)
            }
        };

        await dbContext.Set<DraftFormViewRow>().AddRangeAsync(rows);

        _ = await dbContext.SaveChangesAsync();

        var expected = rows.Select(row => new DraftListItem(
            row.Id,
            row.Description,
            row.Modifier,
            row.PublishDate,
            Enum.Parse<ArticleStatus>(row.Status),
            row.CreatedOn,
            row.UpdatedOn))
            .ToArray()
            .Reverse();

        // Act
        var actual = await sut.ListDraftFormsAsync(brandId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, await actual.Items.ToArrayAsync());
    }

    [Fact]
    public async Task 列表草稿及文章_回傳所有資料()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                        _ = dbContextOptionsBuilder
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(x => x.Ignore(
                                InMemoryEventId.TransactionIgnoredWarning))
                            .ReplaceService<
                                IQueryTranslationPreprocessorFactory,
                                TestInMemoryQueryTranslationPreprocessorFactory<ListDraftFormsQueryTranslationPreprocessor>>();

                        return new TgsContext(dbContextOptionsBuilder.Options);
                    }))));

        var httpClient = web.CreateClient();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var brandId = Guid.NewGuid();

        var currentTime = DateTime.UtcNow;
        var rows = Enumerable.Range(0, 110)
            .Select(n => new DraftFormViewRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                Modifier = "system",
                Status = nameof(ArticleStatus.Draft),
                UpdatedOn = currentTime.AddMinutes(n)
            })
            .ToArray();

        await dbContext.Set<DraftFormViewRow>().AddRangeAsync(rows);

        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.ListDraftFormsAsync(brandId);
        var items = await actual.Items.ToArrayAsync();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(110, items.Length);
        rows.Select(row => row.Id).ToArray()
            .ToExpectedObject()
            .ShouldMatch(items.Select(item => item.Id).ToArray());
    }

    [Fact]
    public async Task 列表草稿及文章_透過skip取得分頁資料()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                        _ = dbContextOptionsBuilder
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(x => x.Ignore(
                                InMemoryEventId.TransactionIgnoredWarning))
                            .ReplaceService<
                                IQueryTranslationPreprocessorFactory,
                                TestInMemoryQueryTranslationPreprocessorFactory<ListDraftFormsQueryTranslationPreprocessor>>();

                        return new TgsContext(dbContextOptionsBuilder.Options);
                    }))));

        var httpClient = web.CreateClient();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        var sut = new ArticleManagementClient(httpClient);

        var brandId = Guid.NewGuid();

        var currentTime = DateTime.UtcNow;
        var rows = Enumerable.Range(0, 110)
            .Select(n => new DraftFormViewRow
            {
                Id = Guid.NewGuid(),
                BrandId = brandId,
                Description = "desc",
                Modifier = "system",
                Status = nameof(ArticleStatus.Draft),
                UpdatedOn = currentTime.AddMinutes(n)
            })
            .ToArray();

        await dbContext.Set<DraftFormViewRow>().AddRangeAsync(rows);

        _ = await dbContext.SaveChangesAsync();

        var expected = rows.Take(^20..^10)
            .Select(row => new DraftListItem(
                row.Id,
                row.Description,
                row.Modifier,
                row.PublishDate,
                Enum.Parse<ArticleStatus>(row.Status),
                row.CreatedOn,
                row.UpdatedOn))
            .ToArray()
            .Reverse();

        // Act
        var result = await sut.ListDraftFormsAsync(brandId, 10);
        var actual = await result.Items.Take(10).ToArrayAsync();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
        Assert.Equal(rows[99].Id, actual[0].Id);
    }

    [Fact]
    public async Task 列表草稿及文章_取得查詢筆數()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddHttpClientLoggingHandler()
                    .Replace(ServiceDescriptor.Singleton(sp =>
                    {
                        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                        _ = dbContextOptionsBuilder
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(x => x.Ignore(
                                InMemoryEventId.TransactionIgnoredWarning));

                        return new TgsContext(dbContextOptionsBuilder.Options);
                    }))
                    .AddSingleton(fakeTgsRepository)));

        var httpClient = web.CreateClient();

        var sut = new ArticleManagementClient(httpClient);

        var brandId = Guid.NewGuid();

        _ = fakeTgsRepository.CountDraftFormsAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(123);

        // Act
        var actual = await sut.CountDraftFormsAsync(brandId);

        // Assert
        Assert.Equal(123, actual);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<暫止>")]
    private class ListDraftFormsQueryTranslationPreprocessor : InMemoryQueryTranslationPreprocessor
    {
        public ListDraftFormsQueryTranslationPreprocessor(
            QueryTranslationPreprocessorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
        }

        public override Expression Process(Expression query)
        {
            var extendVisitor = new ReplaceFromSqlExpressionVisitor();
            var exp = extendVisitor.Visit(query);

            return base.Process(exp);
        }

        private class ReplaceFromSqlExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
                => node is FromSqlQueryRootExpression x
                    ? new EntityQueryRootExpression(x.EntityType)
                    : base.VisitExtension(node);
        }
    }
}
