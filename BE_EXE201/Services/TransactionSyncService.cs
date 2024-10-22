using BE_EXE201.Entities;
using Net.payOS.Errors;
using Net.payOS.Types;
using Net.payOS;
using Microsoft.EntityFrameworkCore;


public class TransactionSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionSyncService> _logger;
    private readonly PayOS _payOS;

    public TransactionSyncService(IServiceProvider serviceProvider, ILogger<TransactionSyncService> logger, PayOS payOS)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _payOS = payOS;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncTransactions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while syncing transactions");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every 1 minute
        }
    }

    private async Task SyncTransactions()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        long latestOrderId = await GetLatestOrderId();

        while (true)
        {
            try
            {
                var paymentInfo = await _payOS.getPaymentLinkInformation(latestOrderId + 1);

                // If we get here, a new order exists
                await SaveTransaction(dbContext, paymentInfo);

                if (long.TryParse(paymentInfo.orderCode.ToString(), out long newOrderId))
                {
                    latestOrderId = newOrderId;
                }
                else
                {
                    _logger.LogWarning($"Unable to parse orderCode: {paymentInfo.orderCode}");
                    break;
                }
            }
            catch (PayOSError ex) when (ex.Code == "429")
            {
                // We've reached the rate limit, so we'll stop for now
                _logger.LogWarning("Rate limit reached. Stopping sync process.");
                break;
            }
            catch (PayOSError ex) when (ex.Code == "14")
            {
                // No new orders found, we can stop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing order {latestOrderId + 1}");
                break;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task<long> GetLatestOrderId()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var latestTransaction = await dbContext.PaymentTransactions
            .OrderByDescending(pt => pt.TransactionId)
            .FirstOrDefaultAsync();

        if (latestTransaction != null && long.TryParse(latestTransaction.TransactionId, out long result))
        {
            return result;
        }

        return 0;
    }

    private async Task SaveTransaction(AppDbContext dbContext, PaymentLinkInformation paymentInfo)
    {
        var existingTransaction = await dbContext.PaymentTransactions
            .FirstOrDefaultAsync(pt => pt.TransactionId == paymentInfo.orderCode.ToString());

        if (existingTransaction == null)
        {
            var newTransaction = new PaymentTransaction
            {
                Amount = paymentInfo.amount,
                TransactionId = paymentInfo.orderCode.ToString(),
                Status = paymentInfo.status,
                CreateDate = DateTime.Parse(paymentInfo.createdAt),
                UpdatedDate = DateTime.UtcNow
            };

            dbContext.PaymentTransactions.Add(newTransaction);
        }
        else
        {
            // Optionally update existing transaction if needed
            existingTransaction.Status = paymentInfo.status;
            existingTransaction.UpdatedDate = DateTime.UtcNow;
            dbContext.Entry(existingTransaction).State = EntityState.Modified;
        }
    }
}
