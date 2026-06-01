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

		public async Task<bool> InsertConfigurations(List<Configuration> configs)
		{
			List<Configuration> cachedConfigs = await GetConfigurations();

			cachedConfigs.AddRange(configs);

			_memoryCache.Set(AllConfigKey, cachedConfigs);

			return true;
		}
	}
}
