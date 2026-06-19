using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingController : ControllerBase
    {
        private readonly IReadingService _readingService;

        public ReadingController(IReadingService readingService)
        {
            _readingService = readingService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get([FromQuery] int coinId)
		{
			try
			{
				List<Reading> readings = await _readingService.GetReadings(coinId);
				return Ok(readings);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error fetching readings: {ex.Message}");
				return StatusCode(500, "An error occurred while fetching readings.");
			}
		}

		[HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] List<Reading> readings)
        {
            try
            {
				bool inserted = await _readingService.InsertReadings(readings);
                return Ok(inserted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting readings: {ex.Message}");
                return StatusCode(500, "An error occurred while inserting readings.");
            }
        }
    }
}
