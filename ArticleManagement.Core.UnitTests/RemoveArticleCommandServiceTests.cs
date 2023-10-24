using ExpectedObjects;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;
using Xunit;

namespace ArticleManagement.Core.UnitTests;

public class RemoveArticleCommandServiceTests
{
    [Fact]
    public async Task 下架文章()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new RemoveArticleCommandService(fakeTgsRepository);

        var command = new RemoveArticleCommand(Guid.NewGuid(), "tester");

        _ = fakeTgsRepository.GetArticleFormAsync(Arg.Is(command.FormId))
            .Returns(new ArticleForm(
                command.FormId,
                Guid.NewGuid(),
                "description",
                new Dictionary<string, Article>(),
                ArticleStatus.Submission,
                "test1",
                DateTime.UtcNow));

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .SetArticleStatusAsync(
                Arg.Is(command.FormId),
                Arg.Is(ArticleStatus.Removed),
                Arg.Is(command.Modifier));
    }

    [Fact]
    public async Task 如果沒有找到文章不能下架()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new RemoveArticleCommandService(fakeTgsRepository);

        var command = new RemoveArticleCommand(Guid.NewGuid(), "tester");

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(0)
            .SetArticleStatusAsync(
                Arg.Is(command.FormId),
                Arg.Is(ArticleStatus.Removed),
                Arg.Is(command.Modifier));
    }
}
