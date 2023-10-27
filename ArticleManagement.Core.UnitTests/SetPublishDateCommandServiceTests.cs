using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class SetPublishDateCommandServiceTests
{
    [Fact]
    public async Task 設定文章發行日期()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SetPublishDateCommandService(fakeTgsRepository);

        var command = new SetPublishDateCommand(
            Guid.NewGuid(),
            DateOnly.Parse("2023-10-4"),
            "modifier");

        _ = fakeTgsRepository.GetDraftFormAsync(Arg.Is(command.FormId))
            .Returns(new DraftForm(
                command.FormId,
                Guid.NewGuid(),
                "desc",
                new Dictionary<string, Article>(),
                "system",
                ArticleStatus.Draft,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow));

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .PatchDraftAsync(Arg.Is(DraftForm.Empty with
            {
                Id = command.FormId,
                PublishDate = command.PublishDate,
                Status = ArticleStatus.WaitingSubmission,
                Modifier = command.Modifier
            }));
    }

    [Fact]
    public async Task 設定文章發行日期_找不到資料時不動作()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SetPublishDateCommandService(fakeTgsRepository);

        var command = new SetPublishDateCommand(
            Guid.NewGuid(),
            DateOnly.Parse("2023-10-4"),
            "modifier");

        _ = fakeTgsRepository.GetDraftFormAsync(Arg.Is(command.FormId))
            .Returns(default(DraftForm));

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(0)
            .PatchDraftAsync(Arg.Is(DraftForm.Empty with
            {
                Id = command.FormId,
                PublishDate = command.PublishDate,
                Status = ArticleStatus.WaitingSubmission,
                Modifier = command.Modifier
            }));
    }

    [Fact]
    public async Task 設定文章發行日期_如果已達20篇限制_會發生限制錯誤()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SetPublishDateCommandService(fakeTgsRepository);

        var draftForm = new DraftForm(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "desc",
            new Dictionary<string, Article>(),
            "system",
            ArticleStatus.Draft,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);

        var command = new SetPublishDateCommand(
            draftForm.Id,
            DateOnly.Parse("2023-10-4"),
            "modifier");

        _ = fakeTgsRepository.GetDraftFormAsync(Arg.Is(draftForm.Id))
            .Returns(draftForm);

        _ = fakeTgsRepository.CountPublishArticleAsync(Arg.Is(draftForm.BrandId))
            .Returns(20);

        // Act
        _ = await Assert.ThrowsAsync<PublishCountExceedLimitException>(
            async () => await sut.ExecuteAsync(command));
    }
}
