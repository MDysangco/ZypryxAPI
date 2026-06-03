using Microsoft.Extensions.Caching.Memory;
using Zyprix.Data.Interfaces;
using Zyprix.Models;

namespace ZypryxAPI.Services.Flushing
{
	public class ReadingFlushService : BackgroundService
	{
		private readonly int _offsetMinutes;
		private readonly IServiceProvider _services;
		private readonly ILogger<ReadingFlushService> _logger;

		public ReadingFlushService(IServiceProvider serviceProvider, ILogger<ReadingFlushService> logger, int offsetMinutes = 0)
		{
			_services = serviceProvider;
			_logger = logger;
			_offsetMinutes = offsetMinutes;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var now = DateTime.Now;

				var oneAM = now.Date.AddHours(1).AddMinutes(_offsetMinutes);
				var onePM = now.Date.AddHours(13).AddMinutes(_offsetMinutes);

				DateTime nextRun;

				if (now < oneAM)
				{
					nextRun = oneAM;
				}
				else if (now < onePM)
				{
					nextRun = onePM;
				}
				else
				{
					nextRun = oneAM.AddDays(1);
				}

				var delay = nextRun - now;
				if (delay < TimeSpan.Zero)
				{
					delay = TimeSpan.Zero;
				}

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
