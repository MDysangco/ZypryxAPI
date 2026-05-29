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

		[HttpPost]
		[Route("")]
		public async Task<IActionResult> Insert([FromBody] List<Reading> reading)
		{
			try
			{
				bool inserted = await _readingService.InsertReading(reading);
				return Ok(inserted);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error inserting readings: {ex.Message}");
				return StatusCode(500, "An error occurred while inserting readings.");
			}
		}

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] Reading reading)
        {
            try
            {
                bool inserted = await _readingService.InsertReading(reading);
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
