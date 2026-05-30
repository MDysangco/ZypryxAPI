using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi;
using Zyprix.Models;
using Zyprix.Services;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Caching
{
	public class CachedKlineService : IKlineService
	{
		private readonly IKlineService _klineService;
		private readonly ICoinService _coinService;
		private readonly IMemoryCache _memoryCache;

		public CachedKlineService(IKlineService klineService, ICoinService coinService, IMemoryCache memoryCache)
		{
			_klineService = klineService;
			_coinService = coinService;
			_memoryCache = memoryCache;
		}

		public async Task<List<Kline>> GetKlines(int? coinId, KlineInterval? interval)
		{
			return await _memoryCache.GetOrCreateAsync($"klines_{coinId}_{interval}", async entry =>
			{
				return await _klineService.GetKlines(coinId, interval) ?? new List<Kline>();
			}) ?? new List<Kline>();
		}

		public async Task<Kline> GetEarliestRecordedKline(int coinId, KlineInterval interval)
		{
			List<Kline>? klines = await GetKlines(coinId, interval);
			return klines?.OrderBy(kline => kline.KlineOpenTime).FirstOrDefault() ?? new Kline();
		}

		public async Task<Kline> GetLatestRecordedKline(int coinId, KlineInterval interval)
		{
			List<Kline>? klines = await GetKlines(coinId, interval);
			return klines?.OrderByDescending(kline => kline.KlineOpenTime).FirstOrDefault() ?? new Kline();
		}

		public async Task<bool> InsertKlines(List<Kline> klines)
		{
			int? coinId = klines.FirstOrDefault()?.CoinId;
			KlineInterval? interval = klines.FirstOrDefault()?.Interval;

			if (interval == null || coinId == null)
			{
				return false;
			}

			List<Kline>? cachedKlines = await GetKlines(coinId, interval);

			if (cachedKlines != null)
			{
				cachedKlines.AddRange(klines);

				_memoryCache.Set($"klines_{coinId}_{interval}", cachedKlines);
			}

			return true;
		}

		public async Task<int> DeleteKlinesByDateRange(long startDate, long endDate)
		{
			List<Coin> coins = await _coinService.GetAllCoins();
			int removedKlines = 0;

			foreach (Coin coin in coins)
			{
				int coinId = coin.Id;
				KlineInterval interval = KlineInterval.OneHour;

				List<Kline>? cachedKlines = await GetKlines(coinId, interval);
				if (cachedKlines == null)
				{
					continue;
				}

				int beforeCount = cachedKlines.Count;

				cachedKlines = cachedKlines.Where(k => k.KlineOpenTime < startDate || k.KlineOpenTime > endDate).ToList();

				int afterCount = cachedKlines.Count;
				removedKlines += beforeCount - afterCount;

				_memoryCache.Set($"klines_{coinId}_{interval}", cachedKlines);
			}

			return removedKlines;
		}
	}

}
