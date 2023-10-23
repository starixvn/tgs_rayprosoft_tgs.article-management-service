using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class ListDraftFormsQueryServiceTests
{
    [Fact]
    public async Task 列出文章及草稿()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new ListDraftFormsQueryService(fakeTgsRepository);

        var query = new ListDraftFormsQuery(Guid.NewGuid());

        // Act
        _ = await sut.QueryAsync(query).ToArrayAsync();

        // Assert
        _ = fakeTgsRepository.Received(1)
            .ListDraftFormsAsync(Arg.Is(query.BrandId), Arg.Is(0));
    }

    [Fact]
    public async Task 列出文章及草稿_取得分頁資料()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new ListDraftFormsQueryService(fakeTgsRepository);

        var query = new ListDraftFormsQuery(Guid.NewGuid(), 100);

        // Act
        _ = await sut.QueryAsync(query).ToArrayAsync();

        // Assert
        _ = fakeTgsRepository.Received(1)
            .ListDraftFormsAsync(Arg.Is(query.BrandId), Arg.Is(query.Skip));
    }

    [Fact]
    public async Task 列出文章及草稿_取得查詢筆數()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new ListDraftFormsQueryService(fakeTgsRepository);

        var query = new ListDraftFormsQuery(Guid.NewGuid());

        // Act
        _ = await sut.GetAsync(query);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .CountDraftFormsAsync(Arg.Is(query.BrandId));
    }
}
