using Elastic.Apm.NetCoreAll;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;

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

builder.Services.AddControllers();

var app = builder.Build();

app.UseAllElasticApm(builder.Configuration);
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
