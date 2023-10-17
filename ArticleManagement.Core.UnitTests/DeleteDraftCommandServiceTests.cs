using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class DeleteDraftCommandServiceTests
{
    [Fact]
    public async Task 刪除草稿()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new DeleteDraftCommandService(fakeTgsRepository);

        var command = new DeleteDraftCommand(Guid.NewGuid());

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .DeleteDraftAsync(Arg.Is(command.FormId));
    }
}
