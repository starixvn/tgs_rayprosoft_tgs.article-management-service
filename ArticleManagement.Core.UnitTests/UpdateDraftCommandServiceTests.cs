using ExpectedObjects;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class UpdateDraftCommandServiceTests
{
    [Fact]
    public async Task 修改文章草稿()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new UpdateDraftCommandService(fakeTgsRepository);

        var command = new UpdateDraftCommand(DraftForm.Empty with
        {
            Id = Guid.NewGuid(),
            Description = "test"
        });

        _ = fakeTgsRepository.GetDraftFormAsync(Arg.Is(command.UpdateData.Id))
            .Returns(DraftForm.Empty);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .PatchDraftAsync(Arg.Is(command.UpdateData));
    }

    [Fact]
    public async Task 修改文章草稿時_如果找不到草稿資料也找不到文章資料_不會執行修改()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new UpdateDraftCommandService(fakeTgsRepository);

        var command = new UpdateDraftCommand(DraftForm.Empty with
        {
            Id = Guid.NewGuid(),
            Description = "test"
        });

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(0)
            .PatchDraftAsync(Arg.Is(command.UpdateData));
    }

    [Fact]
    public async Task 修改文章草稿時_如果找到文章但是沒有草稿_會建立一份草稿再更新()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new UpdateDraftCommandService(fakeTgsRepository);

        var command = new UpdateDraftCommand(DraftForm.Empty with
        {
            Id = Guid.NewGuid(),
            Description = "test",
            UpdatedOn = DateTime.UtcNow
        });

        var articleForm = new ArticleForm(
            command.UpdateData.Id,
            Guid.NewGuid(),
            command.UpdateData.Description,
            command.UpdateData.LocalizedArticles,
            command.UpdateData.Status,
            command.UpdateData.Modifier,
            command.UpdateData.UpdatedOn);

        _ = fakeTgsRepository.GetArticleFormAsync(Arg.Is(command.UpdateData.Id))
            .Returns(articleForm);

        var expected = new DraftForm(
                articleForm.Id,
                articleForm.BrandId,
                articleForm.Description,
                articleForm.LocalizedArticles,
                articleForm.Modifier,
                articleForm.Status,
                null,
                articleForm.UpdatedOn,
                DateTime.UtcNow);
        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .CreateNewDraftAsync(Arg.Is<DraftForm>(form => expected.ToExpectedObject(
                ctx => ctx
                    .Ignore(f => f.UpdatedOn)
                    .IgnoreEqualsOverride()).Matches(form)));
        _ = fakeTgsRepository.Received(1)
            .PatchDraftAsync(Arg.Is(command.UpdateData));
    }
}
