using ExpectedObjects;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class PublishDraftCommandServiceTests
{
    [Fact]
    public async Task 立即從草稿發佈文章()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new PublishDraftCommandService(fakeTgsRepository);

        var command = new PublishDraftCommand(Guid.NewGuid(), "tester");

        var draftForm = new DraftForm(
            command.FormId,
            Guid.NewGuid(),
            "description",
            new Dictionary<string, Article>(),
            "tester1",
            ArticleStatus.Draft,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);

        fakeTgsRepository.GetDraftFormAsync(Arg.Is(command.FormId))
            .Returns(draftForm);

        var expected = new ArticleForm(
            command.FormId,
            draftForm.BrandId,
            draftForm.Description,
            draftForm.LocalizedArticles,
            ArticleStatus.Submission,
            command.Modifier,
            DateTime.Now);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .UpsertArticleFormAsync(Arg.Is<ArticleForm>(
                form => expected.ToExpectedObject(ctx => ctx
                    .Ignore(art => art.UpdatedOn)
                    .IgnoreEqualsOverride()).Matches(form)));
    }
}
