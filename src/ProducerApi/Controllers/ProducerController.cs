using Microsoft.AspNetCore.Mvc;
using ProducerApi.Controllers;

namespace DesignPatterns.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly ILogger<ProducerController> _logger;

        /// <summary>
        /// New instance of controller is created for each api call.
        /// </summary>
        /// <param name="logger"></param>
        public ProducerController(ILogger<ProducerController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Post a message to kafka
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     Post /produce
        ///     "New York"
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
        public ActionResult<int> PostProduce([FromBody] string message)
        {
            try
            {
                var builder = new Boeing747Builder();
                var director = new AirCraftDirector(builder);
                director.Construct(false);
                var boeing747 = builder.GetResult();
                return StatusCode(201, boeing747.Fly(destination));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(500);
            }
        }
    }
}

