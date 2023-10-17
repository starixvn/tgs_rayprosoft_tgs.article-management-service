using ExpectedObjects;
using NSubstitute;
using Tgs.ArticleManagement;
using Tgs.ArticleManagement.ApplicationServices;

namespace ArticleManagement.Core.UnitTests;

public class CreateNewDraftCommandServiceTests
{
    [Fact]
    public async Task 新建編輯中草稿資料()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new CreateNewDraftCommandService(fakeTgsRepository);

        var command = new CreateNewDraftCommand(
            Guid.NewGuid(),
            "description",
            null,
            new Dictionary<string, Article>
            {
                ["en-US"] = new Article(
                    "title",
                    "brief",
                    "content",
                    "writer",
                    new Dictionary<string, Uri>
                    {
                        ["Banner1"] = new Uri("https://xxx.yyy.zz/123.jpg")
                    })
            },
            "modifier");

        var expected = new DraftForm(
            Guid.NewGuid(),
            command.BrandId,
            command.Description,
            command.LocalizedArticles,
            command.Modifier,
            ArticleStatus.Draft,
            command.PublishDate,
            DateTime.Now,
            DateTime.Now).ToExpectedObject(ctx => ctx
                .IgnoreRelativePath(nameof(DraftForm.Id))
                .IgnoreRelativePath(nameof(DraftForm.CreatedOn))
                .IgnoreRelativePath(nameof(DraftForm.UpdatedOn))
                .IgnoreEqualsOverride());

        // Act
        _ = await sut.GetAsync(command);

        // Assert
        _ = fakeTgsRepository.Received(1)
            .CreateNewDraftAsync(
                Arg.Is<DraftForm>(form => expected.Matches(form)));
    }
}
