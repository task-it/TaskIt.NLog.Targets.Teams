using Microsoft.Extensions.Logging;
using System;

namespace NLog.Targets.Teams.Test.App
{
    public class Runner
    {
        private readonly ILogger<Runner> _logger;

        public Runner(ILogger<Runner> logger)
        {
            _logger = logger;
        }

        public void DoActions(string name)
        {
            _logger.LogTrace("Leave no trace behind");
            _logger.LogDebug("Debugging is so nice ... NOT");
            _logger.LogInformation("Important Information");
            _logger.LogWarning("oO ... something is wrong, but I'm not sure");
            _logger.LogError("oO ... something is wrong, definetly");
            throw new InvalidOperationException("It's a trap...");

        }
    }
}
