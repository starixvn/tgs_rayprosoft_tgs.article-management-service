using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class GetDraftFormQueryServiceTests
{
    [Fact]
    public async Task 取得草稿細項內容()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new GetDraftFormQueryService(fakeTgsRepository);

        var query = new GetDraftFormQuery(Guid.NewGuid());

        // Act
        _ = await sut.GetAsync(query);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .GetDraftFormAsync(Arg.Is(query.FormId));
    }
}
