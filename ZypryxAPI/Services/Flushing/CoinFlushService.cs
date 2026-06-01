using Microsoft.Extensions.Caching.Memory;
using Zyprix.Data.Interfaces;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Flushing
{
	public class CoinFlushService : BackgroundService
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<CoinFlushService> _logger;

		private const string AllCoinsKey = "all_coins";

		public CoinFlushService(IServiceProvider serviceProvider, ILogger<CoinFlushService> logger)
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

				nextRun = nextRun.AddMinutes(0);

				var delay = nextRun - now;

				_logger.LogInformation("Next coin flush scheduled for {time}", nextRun);

				await Task.Delay(delay, stoppingToken);

				await Flush(stoppingToken);
			}
		}

		private async Task Flush(CancellationToken token)
		{
			using var scope = _services.CreateScope();

			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var coinRepository = scope.ServiceProvider.GetRequiredService<ICoinRepository>();

			try
			{
				List<Coin>? coins = memoryCache.Get<List<Coin>>(AllCoinsKey);

				if (coins != null)
				{
					foreach (var coin in coins)
					{
						await coinRepository.UpdateCoin(coin);
					}

					memoryCache.Remove(AllCoinsKey);

					_logger.LogInformation("Flushed coins to SQL and cleared cache at {time}", DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during coin flush");
			}
		}
	}
}
