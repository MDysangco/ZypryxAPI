using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Caching
{
	public class CachedReadingService : IReadingService
	{
		private readonly IReadingService _readingService;
		private readonly IMemoryCache _memoryCache;

		public CachedReadingService(IReadingService readingService, IMemoryCache memoryCache)
		{
			_readingService = readingService;
			_memoryCache = memoryCache;
		}

		public async Task<List<Reading>> GetReadings(int coinId)
		{
			return await _memoryCache.GetOrCreateAsync($"readings_{coinId}", async entry =>
			{
				return await _readingService.GetReadings(coinId) ?? new List<Reading>();
			}) ?? new List<Reading>();	
		}

		public async Task<bool> InsertReadings(List<Reading> readings)
		{
			IEnumerable<IGrouping<int, Reading>> groups = readings.GroupBy(r => r.CoinId);

			foreach (var group in groups)
			{
				int coinId = group.Key;
				string cacheKey = $"readings_{coinId}";

				List<Reading> cached = await GetReadings(coinId);

				var existingKeys = new HashSet<(DateTime ts, int modelId)> (
					cached.Select(r => (r.TimeStampUTC, r.ModelId))
				);

				foreach (Reading reading in group)
				{
					var key = (reading.TimeStampUTC, reading.ModelId);

					if (!existingKeys.Contains(key))
					{
						cached.Add(reading);
						existingKeys.Add(key);
					}
				}

				_memoryCache.Set(cacheKey, cached);
			}

			return true;
		}

	}
}
