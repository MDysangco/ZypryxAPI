using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KlineController : ControllerBase
    {
        private readonly IKlineService _klineService;

        public KlineController(IKlineService klineService)
        {
            _klineService = klineService;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get([FromQuery]int? coinId, [FromQuery] KlineInterval? interval)
        {
            try
            {
                List<Kline> klines = await _klineService.GetKlines(coinId, interval);
                return Ok(klines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching klines: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching klines.");
            }
        }

        [HttpPost]
        [Route("insert")]
        public async Task<IActionResult> Insert([FromBody] List<Kline> kline)
        {
            try
            {
                bool klines = await _klineService.InsertKlines(kline);
                return Ok(klines);
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting klines: {ex.Message}");
                return StatusCode(500, "An error occurred while inserting klines.");
            }
        }

        [HttpGet]
        [Route("latest")]
        public async Task<IActionResult> GetLatest([FromQuery] int coinId, [FromQuery] KlineInterval interval)
        {
            try
            {
                Kline klines = await _klineService.GetLatestRecordedKline(coinId, interval);
                return Ok(klines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching latest kline: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching latest kline.");
            }
        }

        [HttpGet]
        [Route("earliest")]
        public async Task<IActionResult> GetEarliest([FromQuery] int coinId, [FromQuery] KlineInterval interval)
        {
            try
            {
                Kline klines = await _klineService.GetEarliestRecordedKline(coinId, interval);
                return Ok(klines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching earliest kline: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching earliest kline.");
            }
        }

    }
}
