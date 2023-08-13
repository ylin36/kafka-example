using Confluent.Kafka;
using ConsumerBackgroundTasks;
using NLog;
using NLog.Extensions.Logging;
using ConsumerBackgroundTasks.Factories;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().GetCurrentClassLogger();
logger.Info("Init program");

IConfiguration configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
configuration["bootstrap.servers"] = "localhost:9092";
configuration["group.id"] = "consumerconsoleapps";
configuration["auto.offset.reset"] = "earliest"; // when no offset exist, reset to earliest
// if auto commit is not wanted, turn it off enable.auto.commit = "false"

//configuration["enable.auto.commit"] = "false";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
        .AddHostedService<Worker>()
        .AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>()
        .AddSingleton(configuration)
        .AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddNLog();
        });
    })
    .Build();

await host.RunAsync();

