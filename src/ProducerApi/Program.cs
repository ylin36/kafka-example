using System.Reflection;
using NLog;
// Microsoft.Extension.Logging DI
using NLog.Extensions.Logging;
using Confluent.Kafka;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().GetCurrentClassLogger();
logger.Info("Init program");

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var kafkaConfigs = new List<string> { "bootstrap.servers" };
    
configuration["bootstrap.servers"] = "localhost:9092";

using var producer = new ProducerBuilder<string, string>(configuration.AsEnumerable().Where(c => kafkaConfigs.Contains(c.Key))).Build();

try
{
    builder.Services
        .AddSingleton<IConfiguration>(configuration)
        .AddSingleton(producer);

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // using System.Reflection to generate swagger document.
        // make sure below is in csproj
        // <PropertyGroup>
        //   <GenerateDocumentationFile>true</GenerateDocumentationFile>
        // </PropertyGroup >
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });
    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddNLog();
    });
    builder.Services.Configure<RouteOptions>(option =>
    {
        option.LowercaseUrls = true;
        option.LowercaseQueryStrings = true;
    });

    var app = builder.Build();

    // use a pathBase url to allow for reverse proxy with routes.
    // also add launchUrl": "design-patterns/swagger" to launchsettings.json to load swagger page on development
    var pathBase = "producer-api";

    app.UsePathBase($"/{pathBase}");

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    logger.Error(e, "Exit program due to exception");
    throw;
}
finally
{
    producer.Flush(TimeSpan.FromSeconds(10));
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}