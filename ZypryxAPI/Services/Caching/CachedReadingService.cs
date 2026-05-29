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

		public async Task<bool> InsertReading(Reading reading)
		{
			List<Reading> readings = await GetReadings(reading.CoinId);
			readings.Add(reading);

			_memoryCache.Set($"readings_{reading.CoinId}", readings);

			return true;
		}

		public async Task<bool> InsertReadings(List<Reading> readings)
		{
			foreach (var reading in readings)
			{
				await InsertReading(reading);
			}

			return true;
		}
	}
}
