using Microsoft.Extensions.Caching.Memory;
using Zyprix.Models;
using Zyprix.Services.Interfaces;

namespace ZypryxAPI.Services.Caching
{
	public class CachedConfiguratiuonService : IConfigurationService
	{
		private readonly IConfigurationService _configurationService;
		private readonly IMemoryCache _memoryCache;

		private const string AllConfigKey = "all_config";


		public CachedConfiguratiuonService(IConfigurationService configurationService, IMemoryCache memoryCache)
		{
			_configurationService = configurationService;
			_memoryCache = memoryCache;
		}

		public async Task<List<Configuration>> GetConfigurations()
		{
			return _memoryCache.Get<List<Configuration>>(AllConfigKey) ?? new List<Configuration>();
		}

		public async Task<int> InsertConfiguration(Configuration config)
		{
			List<Configuration> pending = await GetConfigurations();

			pending.Add(config);

			_memoryCache.Set(AllConfigKey, pending);

			return config.Id;
		}

		public async Task<bool> InsertConfigurations(List<Configuration> configs)
		{
			List<Configuration> pending = await GetConfigurations();

			pending.AddRange(configs);

			_memoryCache.Set(AllConfigKey, pending);

			return true;
		}
	}
}
