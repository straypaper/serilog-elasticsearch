using Elastic.Apm.SerilogEnricher;
using Elastic.Serilog.Enrichers.Web;

using Serilog;
using Serilog.Debugging;

SelfLog.Enable(Console.Out);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", true, true)
	.AddEnvironmentVariables()
	.Build();

builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
{
	var httpAccessor = hostingContext.Configuration.Get<HttpContextAccessor>();

	loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
		.Enrich.FromLogContext()
		.Enrich.WithElasticApmCorrelationInfo()
		.Enrich.WithEcsHttpContext(httpAccessor!)
		.Enrich.WithProperty("service.name", hostingContext.Configuration["ElasticApm.ServiceName"]);
});

builder.Services.AddElasticApmForAspNetCore();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

try
{
	app.Run();
}
finally
{
	Log.CloseAndFlush();
}
