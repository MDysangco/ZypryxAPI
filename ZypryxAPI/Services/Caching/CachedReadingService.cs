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
			foreach (Reading reading in readings)
			{
				string cacheKey = $"readings_{reading.CoinId}";
				if (!_memoryCache.TryGetValue(cacheKey, out List<Reading>? cachedReadings))
				{
					cachedReadings = new List<Reading>();
				}

				cachedReadings?.AddRange(reading);

				_memoryCache.Set(cacheKey, cachedReadings);
			}

			return true;
		}

	}
}
