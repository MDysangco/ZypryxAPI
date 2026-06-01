using Microsoft.Extensions.Caching.Memory;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Caching
{
	public class CachedCoinService : ICoinService
	{
		private readonly ICoinService _coinService;
		private readonly IMemoryCache _memoryCache;

		private const string AllCoinsKey = "all_coins";

		public CachedCoinService(ICoinService coinService, IMemoryCache memoryCache)
		{
			_coinService = coinService;
			_memoryCache = memoryCache;
		}

		public async Task<List<Coin>> GetAllCoins()
		{
			return await _memoryCache.GetOrCreateAsync(AllCoinsKey, async entry =>
			{
				return await _coinService.GetAllCoins() ?? new List<Coin>();
			}) ?? new List<Coin>();
		}

		public async Task<List<Coin>> GetActiveCoins()
		{
			List<Coin> allCoins = await GetAllCoins();
			return allCoins.Where(c => c.Active == true).ToList();
		}

		public async Task<Coin> GetCoin(int coinId)
		{
			List<Coin> coins = await GetAllCoins();
			return coins.FirstOrDefault(c => c.Id == coinId) ?? new Coin();
		}

		public async Task<bool> UpdateCoin(Coin coin)
		{
			List<Coin> coins = await GetAllCoins();

			Coin? existing = coins.FirstOrDefault(c => c.Id == coin.Id);
			if (existing != null && existing.Id == coin.Id)
			{
				existing.Ticker = coin.Ticker;
				existing.Name = coin.Name;
				existing.Address = coin.Address;
				existing.ChainId = coin.ChainId;
				existing.Active = coin.Active;
				existing.BinanceListingDate = coin.BinanceListingDate;
			}

			_memoryCache.Set(AllCoinsKey, coins);

			return true;
		}
	}
}
