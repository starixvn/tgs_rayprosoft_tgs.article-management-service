using ExpectedObjects;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class ScanPublishDraftsCommandServiceTests
{
    [Fact]
    public async Task 將要發行的文章發行出去()
    {
        // Arrange
        var fakeTgsRespository = Substitute.For<ITgsRepository>();

        var sut = new ScanAndPublishDraftsCommandService(fakeTgsRespository);

        var command = new ScanAndPublishDraftsCommand(DateOnly.Parse("2023-10-25"));

        var draftForm = new DraftForm(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test",
            new Dictionary<string, Article>().AsReadOnly(),
            "system",
            ArticleStatus.WaitingSubmission,
            command.ProcessDate,
            DateTime.UtcNow,
            DateTime.UtcNow);
        _ = fakeTgsRespository.ListWaitingSubmissionDraftsAsync(Arg.Is(command.ProcessDate))
            .Returns(new[]
            {
                draftForm
            }.ToAsyncEnumerable());

        var expected = new ArticleForm(
            draftForm.Id,
            draftForm.BrandId,
            draftForm.Description,
            draftForm.LocalizedArticles,
            ArticleStatus.Submission,
            draftForm.Modifier,
            DateTime.UtcNow).ToExpectedObject(ctx => ctx
                .Ignore(o => o.UpdatedOn)
                .IgnoreEqualsOverride());
        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRespository.Received(1)
            .UpsertArticleFormAsync(Arg.Is<ArticleForm>(form => expected.Matches(form)));
        _ = fakeTgsRespository.Received(1)
            .DeleteDraftAsync(Arg.Is(draftForm.Id));
    }
}
