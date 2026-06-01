using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zyprix.Data.Interfaces;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
		}

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] List<Configuration> configurations)
        {
            try
            {
                bool inserted = await _configurationService.InsertConfigurations(configurations);
                return Ok(inserted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting configuration: {ex.Message}");
                return StatusCode(500, "An error occurred while inserting configuration.");
            }
        }

    }
}
