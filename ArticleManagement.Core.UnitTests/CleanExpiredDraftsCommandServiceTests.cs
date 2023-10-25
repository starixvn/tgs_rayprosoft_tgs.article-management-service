using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class CleanExpiredDraftsCommandServiceTests
{
    [Fact]
    public async Task 刪除90天過期的草稿()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new CleanExpiredDraftsCommandService(fakeTgsRepository);

        var command = new CleanExpiredDraftsCommand(DateTime.UtcNow);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .RemoveExpiredDraftsAsync(Arg.Is(command.ExpiredTime));
    }
}
