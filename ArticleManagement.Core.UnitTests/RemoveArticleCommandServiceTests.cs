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
            .SetArticleStatusAsync(Arg.Is(command.FormId), Arg.Is(ArticleStatus.Removed));
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
            .SetArticleStatusAsync(Arg.Is(command.FormId), Arg.Is(ArticleStatus.Removed));
    }

    [Fact]
    public async Task 如果沒有編輯中的草稿會建立一個草稿()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new RemoveArticleCommandService(fakeTgsRepository);

        var command = new RemoveArticleCommand(Guid.NewGuid(), "tester");

        var article = new ArticleForm(
            command.FormId,
            Guid.NewGuid(),
            "description",
            new Dictionary<string, Article>(),
            ArticleStatus.Submission,
            "test1",
            DateTime.UtcNow);
        _ = fakeTgsRepository.GetArticleFormAsync(Arg.Is(command.FormId))
            .Returns(article);

        var expected = new DraftForm(
            command.FormId,
            article.BrandId,
            article.Description,
            article.LocalizedArticles,
            command.Modifier,
            ArticleStatus.Removed,
            null,
            article.UpdatedOn,
            DateTime.UtcNow);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .SetArticleStatusAsync(Arg.Is(command.FormId), Arg.Is(ArticleStatus.Removed));
        _ = fakeTgsRepository.Received(1)
            .CreateNewDraftAsync(Arg.Is<DraftForm>(form => expected.ToExpectedObject(
                ctx => ctx
                    .Ignore(f => f.UpdatedOn)
                    .IgnoreEqualsOverride()).Matches(form)));
    }
}
