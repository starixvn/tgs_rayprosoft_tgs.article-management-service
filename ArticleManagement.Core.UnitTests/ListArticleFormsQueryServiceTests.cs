using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class ListArticleFormsQueryServiceTests
{
    [Fact]
    public async Task 列出Brand文章列表()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new ListArticleFormsQueryService(fakeTgsRepository);

        var query = new ListArticleFormsQuery(Guid.NewGuid());

        // Act
        _ = await sut.QueryAsync(query).ToArrayAsync();

        // Assert
        _ = fakeTgsRepository.Received(1)
            .ListArticleFormsAsync(Arg.Is(query.BrandId));
    }
}
