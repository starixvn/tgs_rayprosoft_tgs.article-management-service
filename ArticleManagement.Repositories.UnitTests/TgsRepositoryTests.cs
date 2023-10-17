using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArticleManagement.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tgs.ArticleManagement;
using Xunit;

namespace ArticleManagement.Repositories.UnitTests;
public class TgsRepositoryTests
{
    [Fact]
    public async Task 更新Draft資料()
    {
        // Arrange
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(config => config.Ignore(
                InMemoryEventId.TransactionIgnoredWarning));
        var dbContext = new TgsContext(dbContextOptionsBuilder.Options);
        var sut = new TgsRepository(dbContext);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        _ = await dbContext.DraftRows.AddAsync(new DraftRow
        {
            Id = formId,
            BrandId = brandId,
            Description = "aaa",
            Modifier = "system",
            Status = "Draft",
            LocalizedArticles = "{}"
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        await sut.PatchDraftAsync(DraftForm.Empty with
        {
            Id = formId,
            BrandId = brandId,
            Description = "test",
            LocalizedArticles = new Dictionary<string, Article>
            {
                ["en-US"] = new Article("aaa", "bbb", "ccc", "ddd", new Dictionary<string, Uri>
                {
                    ["200"] = new Uri("http://localhost/1.jpg")
                })
            }
        });

        // Assert

    }
}
