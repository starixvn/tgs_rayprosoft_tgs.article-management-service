namespace Tgs.ArticleManagement.ApplicationServices;
public record SetPublishDateCommand(Guid FormId, DateOnly PublishDate, string Modifier);
