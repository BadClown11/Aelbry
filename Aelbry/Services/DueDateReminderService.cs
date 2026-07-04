using Aelbry.BL.Notifications;

namespace Aelbry.Web.Services
{
    /// <summary>
    /// Job en segundo plano (Modulo 7) que revisa cada cierto tiempo si hay actividades por
    /// vencer y dispara sus recordatorios (notificacion + correo) via DueDateReminderBL.
    /// </summary>
    public class DueDateReminderService : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);
        private const int DaysAhead = 3;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DueDateReminderService> _logger;

        public DueDateReminderService(IServiceScopeFactory scopeFactory, ILogger<DueDateReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reminderBL = scope.ServiceProvider.GetRequiredService<DueDateReminderBL>();
                    int count = await reminderBL.SendDueRemindersAsync(DaysAhead);

                    if (count > 0)
                    {
                        _logger.LogInformation("Se enviaron {Count} recordatorios de vencimiento.", count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar recordatorios de vencimiento.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }
    }
}
