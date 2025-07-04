using Microsoft.Extensions.Configuration;

namespace OpenManus.Core.Configuration
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T GetSection<T>(string sectionName) where T : new()
        {
            var section = _configuration.GetSection(sectionName);
            if (section == null)
            {
                return new T();
            }

            var config = new T();
            section.Bind(config);
            return config;
        }

        public string? GetValue(string key)
        {
            return _configuration[key];
        }
    }
}
