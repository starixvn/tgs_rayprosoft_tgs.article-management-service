using ArticleManagement.Repositories.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.Clients;
using Xunit;
using Xunit.Abstractions;

namespace Tgs.ArticleManagementService.IntegrationTests;

public class ScheduleTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ScheduleTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    }

    [Fact]
    public async Task 處理現在要上架的文章()
    {
        // Arrange
        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper)));

        var httpClient = web.CreateClient();

        var dbContext = web.Services.GetRequiredService<TgsContext>();

        _ = await dbContext.DraftRows.AddAsync(new DraftRow
        {
            Id = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Description = "test",
            LocalizedArticles = "{}",
            CreatedOn = DateTime.UtcNow,
            Modifier = "system",
            PublishDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).DateTime),
            Status = nameof(ArticleStatus.WaitingSubmission),
            UpdatedOn = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var sut = new ArticleManagementClient(httpClient);

        // Act
        await sut.DoPublishDraftsAsync(null);
        var actual = await dbContext.ArticleRows.CountAsync();

        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public async Task 移除過期的草稿()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        await using var web = ArticleManagementTestHost.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging
                    .AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakeTgsRepository)));

        var httpClient = web.CreateClient();

        var sut = new ArticleManagementClient(httpClient);

        // Act
        await sut.DoRemoveExpiredDraftsAsync();

        // Assert
        var expiredTime = DateTime.UtcNow.AddDays(-90D);
        _ = fakeTgsRepository.Received(1)
            .RemoveExpiredDraftsAsync(
                Arg.Is<DateTime>(time => expiredTime.AddMinutes(-1D) <= time
                    && expiredTime.AddMinutes(1D) >= time),
                Arg.Any<CancellationToken>());
    }
}
