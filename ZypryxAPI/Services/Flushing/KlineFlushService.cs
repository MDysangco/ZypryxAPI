using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.Eventing.Reader;
using Zyprix.Data.Interfaces;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Flushing
{
	public class KlineFlushService : BackgroundService
	{
		private readonly int _offsetMinutes;
		private readonly IServiceProvider _services;
		private readonly ILogger<KlineFlushService> _logger;

		public KlineFlushService(IServiceProvider services, ILogger<KlineFlushService> logger, int offsetMinutes = 0)
		{
			_services = services;
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

				_logger.LogInformation("Next kline flush scheduled for {time}", nextRun);

				await Task.Delay(delay, stoppingToken);

				await Flush(stoppingToken);
				await Prune(stoppingToken);
			}
		}

		private async Task Flush(CancellationToken token)
		{
			using var scope = _services.CreateScope();

			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var coinRepository = scope.ServiceProvider.GetRequiredService<ICoinRepository>();
			var klineRepository = scope.ServiceProvider.GetRequiredService<IKlineRepository>();

			try
			{
				var coins = await coinRepository.GetAllCoins();
				KlineInterval interval = KlineInterval.OneHour;

				foreach (var coin in coins)
				{
					string cacheKey = $"klines_{coin.Id}_{interval}";
					List<Kline>? klines = memoryCache.Get<List<Kline>>(cacheKey);

					if (klines == null || klines.Count == 0)
					{
						continue;
					}

					bool success = await klineRepository.InsertKlines(klines);

					if (!success)
					{
						_logger.LogWarning("Failed to flush klines for coin {coinId} interval {interval}", coin.Id, interval);
						continue;
					}

					memoryCache.Remove(cacheKey);

					_logger.LogInformation(	"Flushed {count} klines for coin {coinId} interval {interval} at {time}", klines.Count, coin.Id, interval, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during kline flush");
			}
		}

		private async Task Prune(CancellationToken cancellationToken)
		{
			using var scope = _services.CreateScope();

			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var coinRepository = scope.ServiceProvider.GetRequiredService<ICoinRepository>();
			var klineRepository = scope.ServiceProvider.GetRequiredService<IKlineRepository>();

			try
			{
				long startDate = 0;
				long endDate = new DateTimeOffset(DateTime.UtcNow.AddYears(-7).AddDays(-2))
					.ToUnixTimeMilliseconds();

				int deleted = await klineRepository.DeleteKlinesByDateRange(startDate, endDate);

				var coins = await coinRepository.GetAllCoins();
				KlineInterval interval = KlineInterval.OneHour;

				foreach (var coin in coins)
				{
					string cacheKey = $"klines_{coin.Id}_{interval}";
					List<Kline>? cached = memoryCache.Get<List<Kline>>(cacheKey);

					if (cached == null)
					{
						continue;
					}

					cached = cached.Where(k => k.KlineOpenTime > endDate).ToList();

					memoryCache.Set(cacheKey, cached);
				}

				_logger.LogInformation("Pruned old klines (DB + cache) up to {endDate}", endDate);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during kline prune");
			}
		}

	}
}
