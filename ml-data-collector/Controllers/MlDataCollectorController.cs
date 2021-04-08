using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ml_data_collector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ml_data_collector.Controllers
{
    [Route("api/ml-data-collector")]
    [ApiController]
    public class MlDataCollectorController : ControllerBase
    {

        private readonly ILogger<MlDataCollectorController> _logger;
        private readonly MlDataCollectorDBContexts _context;

        public MlDataCollectorController(MlDataCollectorDBContexts context, ILogger<MlDataCollectorController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MlDataCollector>>> Get() =>
             Ok(await _context.MlDataCollector.ToListAsync());


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MlDataCollector>>> GetById(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return BadRequest();

            var x = await _context.MlDataCollector.Where(p => p.ProzorroHash == id || p.TenderId == id)?.ToListAsync();
            if (x?.Count == 0)
                return NotFound();
            return Ok(x);
        }


        [HttpGet("new")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<ActionResult<MlDataCollector>> NewAsync([FromQuery] string TenderId, string ProzorroHash, bool IsGood)
        {
            try
            {
                // exit if exists
                MlDataCollector _mlDataCollectorItem = _context.MlDataCollector.Where(x => x.ProzorroHash == ProzorroHash).FirstOrDefault();

                if (_mlDataCollectorItem != null)
                {
                    if (_mlDataCollectorItem?.IsGood != IsGood)
                    {
                        // update IsGood property
                        _mlDataCollectorItem.IsGood = IsGood;
                        var x = await _context.SaveChangesAsync();
                        _logger?.LogInformation($"Update MlDataCollector record with id { _mlDataCollectorItem.Id } return { x }");
                    }
                }
                else
                {
                    // add new record
                    _mlDataCollectorItem = new MlDataCollector()
                    {
                        CreatedAt = DateTime.Now,
                        ProzorroHash = ProzorroHash,
                        TenderId = TenderId,
                        IsGood = IsGood
                    };
                    await _context.MlDataCollector.AddAsync(_mlDataCollectorItem);
                    var x = await _context.SaveChangesAsync();
                    _logger?.LogInformation($"Add MlDataCollector record with id { _mlDataCollectorItem.ProzorroHash } return { x }");
                }

                return Redirect("/ui/index.html");

            }
            catch (Exception)
            {
                _logger?.LogError($"Error creating new MlDataCollector record with id { ProzorroHash }");
                return Redirect("/ui/error.html");
            }
        }


        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MlDataCollector>> CreateAsync([FromBody] MlDataCollector message)
        {
            try
            {
                if (message == null)
                    return BadRequest();

                // exit if exists
                MlDataCollector _mlDataCollectorItem = _context.MlDataCollector.Where(x => x.ProzorroHash == message.ProzorroHash).FirstOrDefault();

                if (_mlDataCollectorItem?.IsGood == message.IsGood)
                    return BadRequest();
                       
                if (_mlDataCollectorItem != null)
                {
                    // update IsGood property
                    _mlDataCollectorItem.IsGood = message.IsGood;
                    var x = await _context.SaveChangesAsync();
                    _logger?.LogInformation($"Update MlDataCollector record with id { _mlDataCollectorItem.ProzorroHash } return { x }");
                } else
                {
                    // add new record
                    message.CreatedAt = DateTime.Now;
                    await _context.MlDataCollector.AddAsync(message);
                    var x = await _context.SaveChangesAsync();
                    _logger?.LogInformation($"Add MlDataCollector record with id { message.ProzorroHash } return { x }");
                }

                return CreatedAtAction(nameof(GetById), 
                    new { ProzorroHash = message.ProzorroHash }, message);
            }
            catch (Exception)
            {
                _logger?.LogError($"Error creating new MlDataCollector record with id { message.ProzorroHash }");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error creating new MlDataCollector record with id { message.ProzorroHash }");
            }
        }
    }
}
