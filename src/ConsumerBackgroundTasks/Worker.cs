using System;
using System.Dynamic;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using ConsumerBackgroundTasks.Factories;

namespace ConsumerBackgroundTasks;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IKafkaConsumerFactory _kafkaConsumerFactory;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IKafkaConsumerFactory kafkaConsumerFactory, IConfiguration config)
    {
        _logger = logger;
        _kafkaConsumerFactory = kafkaConsumerFactory;
        _config = config;
    }

    // checkout ActionBlock<T>
    // BufferBlock<T> 
    // BlockingCollection<T> if needed thread safe queue that blocks if theres nothing else to take

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started background task");
        var kafkaConfigs = new List<string> { "bootstrap.servers", "group.id", "auto.offset.reset", "enable.auto.commit" };
        var config = _config.AsEnumerable().Where(c => kafkaConfigs.Contains(c.Key));

        using (var consumer = _kafkaConsumerFactory.CreateKafkaConsumer<string, string>(config))
        {
            _logger.LogInformation("Created new consumer connection");
            try
            {
                var topic = "testtopic";
                consumer.Subscribe(topic);

                _logger.LogInformation($"Subscribed to topic {topic}");

                // process the consume result in another thread
                var actionBlock = new ActionBlock<ConsumeResult<string, string>>(HandleMessageAsync,
                new ExecutionDataflowBlockOptions
                {
                    // 3 threads to process in parallel
                    MaxDegreeOfParallelism = 3,
                    BoundedCapacity = 3
                });

                while (!stoppingToken.IsCancellationRequested)
                {
                    // consume happens in the background.
                    // this consume just takes the latest message in the internal queue / there isn't a consume async
                    var result = consumer.Consume(stoppingToken);

                    _logger.LogInformation($"Received message {result.Message.Value} timestamp: {result.Message.Timestamp} , sending it for processing");

                    // send for actionblock for processing
                    // https://stackoverflow.com/questions/62016856/avoiding-use-of-actionblocktinput-post-when-postdataflowblockoptions-boundedca
                    // Post returns false when
                    // The block is marked as completed (by calling its Complete method).
                    // The block is completed, either successfully or unsuccessfully(its Completion.IsCompleted property returns true).
                    // The block has a bounded capacity(option BoundedCapacity != -1), and its buffer is currently full.
                    // DO NOT USE Post since we don't want a false when bound is full. we want to block until it is available again
                    // actionBlock.Post(result);

                    // The block was marked as completed either before calling the SendAsync, or during the awaiting.
                    // The block was completed either before calling the SendAsync, or during the awaiting as a result of an exception, or because its Fault method was invoked.
                    // using SendSync to await until BoundedCapacity is free again. See ConsumerTest TPLTests for details.
                    await actionBlock.SendAsync(result);

                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("Closing consumer connection");
                consumer.Close();
            }
        }
    }

    private async Task HandleMessageAsync(ConsumeResult<string, string> result)
    {
        _logger.LogInformation($"Consumed {result.Message.Value} from Partiton {result.Partition}");
        await Task.Delay(100);
    }
                
}

