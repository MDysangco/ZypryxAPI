using Microsoft.Extensions.Caching.Memory;
using Zyprix.Data.Interfaces;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Flushing
{
	public class ConfigurationFlushService : BackgroundService
	{
		private readonly int _offsetMinutes;
		private readonly IServiceProvider _services;
		private readonly ILogger<ConfigurationFlushService> _logger;

		private const string AllConfigKey = "all_config";

		public ConfigurationFlushService(IServiceProvider serviceProvider, ILogger<ConfigurationFlushService> logger, int offsetMinutes = 0)
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

				_logger.LogInformation("Next coin flush scheduled for {time}", nextRun);

				await Task.Delay(delay, stoppingToken);

				await Flush(stoppingToken);
			}
		}

		private async Task Flush(CancellationToken token)
		{
			using var scope = _services.CreateScope();

			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var configurationRepository = scope.ServiceProvider.GetRequiredService<IConfigurationRepository>();

			try
			{
				List<Configuration>? configurations = memoryCache.Get<List<Configuration>>(AllConfigKey);
				if (configurations != null) {
					await configurationRepository.InsertConfigurations(configurations);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during coin flush");
			}
		}

	}
}
