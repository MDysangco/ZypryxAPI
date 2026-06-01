using Microsoft.Extensions.Caching.Memory;
using Zyprix.Data.Interfaces;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Flushing
{
	public class ConfigurationFlushService : BackgroundService
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<ConfigurationFlushService> _logger;

		private const string AllConfigKey = "all_config";

		public ConfigurationFlushService(IServiceProvider serviceProvider, ILogger<ConfigurationFlushService> logger)
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

				nextRun = nextRun.AddMinutes(5);

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
