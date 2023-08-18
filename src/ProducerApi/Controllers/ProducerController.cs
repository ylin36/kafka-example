using Microsoft.AspNetCore.Mvc;
using ProducerApi.Controllers;
using Confluent.Kafka;
using ProducerApi.Models;

namespace DesignPatterns.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly ILogger<ProducerController> _logger;
        private readonly IProducer<string, string> _kafkaProducer;
        private readonly KafkapubsubContext _dbContext;

        /// <summary>
        /// New instance of controller is created for each api call, so the none threadsafe DI here can be in general used safely
        /// </summary>
        /// <param name="logger"></param>
        public ProducerController(ILogger<ProducerController> logger, IProducer<string, string> kafkaProducer, KafkapubsubContext dbContext)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _dbContext = dbContext;
            }

        /// <summary>
        /// Post a message to kafka
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     Post /produce
        ///     {
        ///         "topic": "testtopic"
        ///         "key": "testkey",
        ///         "value": "testvalue"  
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Record created</response>
        /// <response code="400">Bad Input</response>
        /// <response code="500">unexcepted internal error</response>
        [HttpPost("produce")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> PostProduce([FromBody] RequestMessage message)
        {
            try
            {
                _logger.LogInformation("start producing");
                var result = await _kafkaProducer.ProduceAsync(message.Topic, new Message<string, string>() { Key = message.Key, Value = message.Value });

                _logger.LogInformation($"Created {result.Value}, on partition {result.Partition.Value} topic {result.Topic} offset {result.Offset.Value}");
                return StatusCode(201, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Post a message to kafka
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     Post /produce
        ///     {
        ///         "topic": "testtopic"
        ///         "key": "testkey",
        ///         "value": "testvalue"  
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Record created</response>
        /// <response code="400">Bad Input</response>
        /// <response code="500">unexcepted internal error</response>
        [HttpPost("produce-persist")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> PostProducePersist([FromBody] RequestMessage message)
        {
            try
            {
                _logger.LogInformation("start producing");
                var portfolio = new Portfolio
                {
                    Description = message.Topic
                };
                portfolio.Products.Add(new Product
                {
                    Payload = $"{{\"value\": \"{message.Value}\"}}"
                });
                _dbContext.Portfolios.Add(portfolio);
                int dbRet = await _dbContext.SaveChangesAsync();
                if (dbRet > 1)
                {
                    var result = await _kafkaProducer.ProduceAsync(message.Topic, new Message<string, string>() { Key = message.Key, Value = message.Value });

                    _logger.LogInformation($"Created {result.Value}, on partition {result.Partition.Value} topic {result.Topic} offset {result.Offset.Value}");
                    return StatusCode(201, result);
                }
                throw new Exception("Failed to add");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(500);
            }
        }
    }
}

