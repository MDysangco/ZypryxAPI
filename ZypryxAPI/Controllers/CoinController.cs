using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CoinController : ControllerBase
    {
        private readonly ICoinService _coinService;

        public CoinController(ICoinService coinService)
        {
            _coinService = coinService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAllCoins()
        {
            try
            {
                List<Coin> coins = await _coinService.GetAllCoins();
                return Ok(coins);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all coins: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching all coins.");
            }
        }


        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveCoins()
        {
            try
            {
                List<Coin> coins = await _coinService.GetActiveCoins();
                return Ok(coins);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching active coins: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching active coins.");
            }
        }

        [HttpGet("{coinId:int}")]
        public async Task<IActionResult> GetCoin(int coinId)
        {
            try
            {
                Coin coin = await _coinService.GetCoin(coinId);
                return Ok(coin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching coin: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching the coin.");
            }
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateCoin([FromBody] Coin coin)
        {
            try
            {
                bool active = coin.Active ?? false;
                long binanceListingDate = coin.BinanceListingDate ?? 0;

                bool success = await _coinService.UpdateCoin(coin, active, binanceListingDate);

                return Ok(success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching coin: {ex.Message}");
                return StatusCode(500, "Failed to update coin.");
            }
        }
    }
}
