using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DonationManagementSystem.Web.BackgroundServices
{
    public class DonationCaseAutoCloseService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DonationCaseAutoCloseService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("DonationCaseAutoCloseService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var approvedCases = await db.DonationCases
                        .Where(c => c.Status == CaseStatus.Approved)
                        .ToListAsync(stoppingToken);

                    foreach (var c in approvedCases)
                    {
                        var collected = await db.Donations
                            .Where(d => d.DonationCaseId == c.Id)
                            .SumAsync(d => (decimal?)d.Amount, stoppingToken) ?? 0m;

                        if (collected >= c.TargetAmount)
                        {
                            c.Status = CaseStatus.Completed;
                            c.AdminNote ??= "Target reached automatically (system).";
                            c.ReviewedAt ??= DateTime.UtcNow;
                            c.ReviewedByUserId ??= "SYSTEM";

                            Log.Information(
                                "AUTO-CLOSED CaseId={CaseId}, Title={Title}",
                                c.Id, c.Title);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "DonationCaseAutoCloseService error");
                }

                // 🔁 For testing: runs every 10 seconds
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            Log.Information("DonationCaseAutoCloseService stopped.");
        }
    }
}
