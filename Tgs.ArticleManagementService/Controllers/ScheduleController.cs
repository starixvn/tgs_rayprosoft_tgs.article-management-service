using Microsoft.AspNetCore.Mvc;
using Tgs.ArticleManagement.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.ArticleManagementService.Controllers;

[Route("[controller]")]
[ApiController]
public class ScheduleController : ControllerBase
{
	[HttpPost("publish", Name = "DoPublishDrafts")]
	public ValueTask DoPublishDraftsAsync(
		[FromServices] ICommandService<ScanAndPublishDraftsCommand> publishService)
	{
		var currentTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D));

		var processDate = DateOnly.FromDateTime(currentTime.DateTime);

		var command = new ScanAndPublishDraftsCommand(processDate);

		return publishService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPost("publish/{processDate}", Name = "DoPublishDraftsByProcessDate")]
	public ValueTask DoPublishDraftsAsync(
		[FromServices] ICommandService<ScanAndPublishDraftsCommand> publishService,
		DateOnly processDate)
	{
		var command = new ScanAndPublishDraftsCommand(processDate);

		return publishService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPost("expired-draft", Name = "DoCleanExpiredDrafts")]
	public ValueTask DoCleanExpiredDraftsAsync(
		[FromServices] ICommandService<CleanExpiredDraftsCommand> cleanService)
	{
		var cleanTime = DateTime.UtcNow.AddDays(-90D);

		var command = new CleanExpiredDraftsCommand(cleanTime);

		return cleanService.ExecuteAsync(command, HttpContext.RequestAborted);
	}
}
