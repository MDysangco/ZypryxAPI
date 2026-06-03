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
		private readonly IMemoryCache _memoryCache;

		public CachedKlineService(IKlineService klineService, IMemoryCache memoryCache)
		{
			_klineService = klineService;
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
			KlineInterval interval = KlineInterval.OneHour;

			if (coinId == null)
			{
				return false;
			}

			List<Kline>? cachedKlines = await GetKlines(coinId, interval);

			if (cachedKlines != null)
			{
				HashSet<long> existingTimes = new HashSet<long>(cachedKlines.Select(k => k.KlineOpenTime.GetValueOrDefault()));

				foreach (Kline kline in klines)
				{
					if (kline.KlineOpenTime.HasValue && !existingTimes.Contains(kline.KlineOpenTime.Value))
					{
						cachedKlines.Add(kline);
						existingTimes.Add(kline.KlineOpenTime.Value);
					}
				}

				_memoryCache.Set($"klines_{coinId}_{interval}", cachedKlines);
			}

			return true;
		}

	}

}
