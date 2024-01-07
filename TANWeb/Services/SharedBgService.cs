using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Services
{

    [ExcludeFromCodeCoverage]
    public class SharedBgService
    {
        private readonly TimedHostedService _timedHostedService;

        public SharedBgService(TimedHostedService timedHostedService)
        {
            _timedHostedService = timedHostedService;
        }

        public void TriggerDoWork()
        {
            _timedHostedService.DoWork("Manual Trigger"); // Pass any relevant parameter if needed
        }
    }

}
