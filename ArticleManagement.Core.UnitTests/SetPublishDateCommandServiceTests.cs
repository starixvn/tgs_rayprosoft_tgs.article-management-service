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
}
