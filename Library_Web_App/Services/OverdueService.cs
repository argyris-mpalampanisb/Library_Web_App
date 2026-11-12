using Library_Web_App.Data;
using Microsoft.EntityFrameworkCore;

namespace Library_Web_App.Services;

public class OverdueService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OverdueService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    public OverdueService(IServiceProvider serviceProvider, ILogger<OverdueService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

                var overdue = await db.Rentals
                    .Include(r => r.Book)
                    .Include(r => r.User)
                    .Where(r => r.ReturnDate == null && r.DueDate < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (overdue.Count > 0)
                {
                    _logger.LogInformation(
                        "⚠️ {Count} overdue rental(s) detected. Users: {Users}",
                        overdue.Count,
                        string.Join(", ", overdue.Select(r => r.User.Username).Distinct()));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue rentals.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}