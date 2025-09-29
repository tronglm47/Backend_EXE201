using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run cleanup every hour

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TokenCleanupService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoCleanupWork();
                    await Task.Delay(_period, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("TokenCleanupService is stopping due to cancellation");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token cleanup");
                    // Continue running even if cleanup fails
                    await Task.Delay(_period, stoppingToken);
                }
            }

            _logger.LogInformation("TokenCleanupService stopped");
        }

        private async Task DoCleanupWork()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var emailVerificationRepo = scope.ServiceProvider.GetRequiredService<IEmailVerificationRepository>();
                var passwordResetRepo = scope.ServiceProvider.GetRequiredService<IPasswordResetRepository>();
                var refreshTokenRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

                try
                {
                    _logger.LogInformation("Starting token cleanup process");

                    // Cleanup expired email verification tokens
                    await emailVerificationRepo.DeleteExpiredTokensAsync();
                    _logger.LogDebug("Cleaned up expired email verification tokens");

                    // Cleanup expired password reset tokens
                    await passwordResetRepo.DeleteExpiredTokensAsync();
                    _logger.LogDebug("Cleaned up expired password reset tokens");

                    // Cleanup expired refresh tokens
                    await refreshTokenRepo.DeleteExpiredTokensAsync();
                    _logger.LogDebug("Cleaned up expired refresh tokens");

                    _logger.LogInformation("Token cleanup completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during token cleanup process");
                    throw; // Re-throw to be caught by the outer try-catch
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TokenCleanupService is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}