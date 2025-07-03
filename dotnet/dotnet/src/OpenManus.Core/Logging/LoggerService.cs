using Microsoft.Extensions.Logging;

namespace OpenManus.Core.Logging
{
    public class LoggerService
    {
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.LogError(ex, message);
        }

        public void LogCritical(string message)
        {
            _logger.LogCritical(message);
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }
    }
}