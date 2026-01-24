using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DonationManagementSystem.Web.BackgroundServices
{
    public class DonationCaseMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DonationCaseMonitorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("DonationCaseMonitorService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var cases = await db.DonationCases
                        .Where(c => c.Status == CaseStatus.Approved)
                        .ToListAsync(stoppingToken);

                    foreach (var c in cases)
                    {
                        var collected = await db.Donations
                            .Where(d => d.DonationCaseId == c.Id)
                            .SumAsync(d => (decimal?)d.Amount, stoppingToken) ?? 0m;

                        if (collected >= c.TargetAmount)
                        {
                            Log.Information(
                                "TARGET REACHED: CaseId={CaseId}, Title={Title}, Collected={Collected}, Target={Target}",
                                c.Id, c.Title, collected, c.TargetAmount
                            );

                            // optional: auto-note (safe, no schema change)
                            if (string.IsNullOrEmpty(c.AdminNote))
                            {
                                c.AdminNote = "Target amount reached automatically.";
                                c.ReviewedAt = DateTime.UtcNow;
                            }
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in DonationCaseMonitorService.");
                }

                // run every 60 seconds (change to 10 sec for testing)
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            }

            Log.Information("DonationCaseMonitorService stopped.");
        }
    }
}
