using Microsoft.Extensions.Caching.Memory;
using Zyprix.Data.Interfaces;
using Zyprix.Models;

namespace ZypryxAPI.Services.Flushing
{
	public class ReadingFlushService : BackgroundService
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<ReadingFlushService> _logger;

		public ReadingFlushService(IServiceProvider serviceProvider, ILogger<ReadingFlushService> logger)
		{
			_services = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var now = DateTime.Now;

				var noon = now.Date.AddHours(12);
				var midnight = now.Date.AddDays(1);

				var nextRun = now < noon ? noon : midnight;

				nextRun = nextRun.AddMinutes(10);

				var delay = nextRun - now;

				_logger.LogInformation("Next reading flush scheduled for {time}", nextRun);

				await Task.Delay(delay, stoppingToken);

				await Flush(stoppingToken);
			}
		}

		private async Task Flush(CancellationToken token)
		{
			using var scope = _services.CreateScope();

			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var coinRepository = scope.ServiceProvider.GetRequiredService<ICoinRepository>();
			var readingRepository = scope.ServiceProvider.GetRequiredService<IReadingRepository>();

			try
			{
				List<Coin> coins = await coinRepository.GetAllCoins();

				foreach (Coin coin in coins)
				{
					string cacheKey = $"readings_{coin.Id}";

					List<Reading>? readings = memoryCache.Get<List<Reading>>(cacheKey);
					if(readings == null || !readings.Any())
					{
						continue;
					}

					bool success = await readingRepository.InsertReadings(readings);
					if(!success)
					{
						_logger.LogError("Failed to insert readings for coin {coinId}", coin.Id);
					}

					memoryCache.Remove(cacheKey);

					_logger.LogInformation("Flushed readings to SQL and cleared cache at {time}", DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while flushing readings");
			}
		}


	}
}
