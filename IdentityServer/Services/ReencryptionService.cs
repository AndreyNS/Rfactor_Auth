
namespace IdentityServer.Services
{
    public class ReencryptionService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReencryptionService> _logger;

        public ReencryptionService(IServiceScopeFactory scopeFactory, ILogger<ReencryptionService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
