using System.Reflection;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Logging
		.AddOpenTelemetry(options =>
		{
			options.IncludeFormattedMessage = true;
			options.ParseStateValues = true;
			_ = options.AddFluentdForwardExporter(fluentOptions =>
			{
				var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
				fluentOptions.Host = nodeIp;
				fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
				fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
					"MachineName",
					() => Environment.MachineName));
			});
		});

	builder.Services
		.AddRouting(routeOptions => routeOptions.LowercaseUrls = true)
		.AddProblemDetails()
		.AddControllers()
		.AddMvcOptions(mvcOptions => mvcOptions
			.AddWebApiToolkitFilters());

	builder.Services
		.AddArticleManagement()
		.AddArticleManagementRepositories(contextBuilder => contextBuilder
			.UseNpgsql("name=Tgs"));

	builder.Services
		.AddHealthChecks()
		.Services
		.AddOpenTelemetry()
		.WithTracing(builder => builder
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
			.AddSource("Tgs.*")
			.AddSource("TGS.*")
			.AddSource("Valhalla.*")
			.AddAspNetCoreInstrumentation()
			.AddProfileViewExporter()
			.AddOtlpExporter())
		.WithMetrics(meterBuiler => meterBuiler
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
				.AddRuntimeInstrumentation()
				.AddAspNetCoreInstrumentation()
				.AddOtlpExporter())
		.Services
		.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
			httpContext => !httpContext.Request.Path.HasValue
				|| !httpContext.Request.Path.StartsWithSegments("/swagger")
				&& !httpContext.Request.Path.StartsWithSegments("/health")
				&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
				&& !httpContext.Request.Path.StartsWithSegments("/metrics")
				&& !new[] { "html", "ico", "png", "jpg" }.Any(
					ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
		.AddProfileViewer();

	_ = builder.Services
		.AddSwaggerGen(genOptions =>
		{
			genOptions.SwaggerDoc(
				"v1",
				new OpenApiInfo
				{
					Version = "v1",
					Title = "ArticleManagement Service"
				});

			genOptions.ApplyWebApiToolkitSupport();

			var xmlFilename = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
			if (File.Exists(xmlFilename))
				genOptions.IncludeXmlComments(xmlFilename);
		});
}

var app = builder.Build();
{
	app.MapControllers();
	app.MapSwagger();
	app.UseSwaggerUI();
	app.MapHealthChecks("/healthz");
}

app.Run();
