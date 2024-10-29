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
    private const int BatchSize = 100; // Adjust this value as needed
    private const int EXPIRATION_HOURS = 12;
    private const string EXPIRED_REASON = "Order expired due to no payment";

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

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task SyncTransactions()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Get expired unpaid transactions
            var expirationDate = DateTime.UtcNow.AddHours(EXPIRATION_HOURS);
            var expiredTransactions = await dbContext.PaymentTransactions
                .Where(pt => pt.Status.ToLower() != "paid"
                         && pt.Status.ToLower() != "cancelled"
                         && pt.CreateDate < expirationDate)
                .Take(BatchSize)
                .ToListAsync();

            foreach (var transaction in expiredTransactions)
            {
                try
                {
                    // Parse the transaction ID to long
                    if (long.TryParse(transaction.TransactionId, out long orderCode))
                    {
                        // Cancel the order in PayOS
                        await _payOS.cancelPaymentLink(orderCode, EXPIRED_REASON);

                        // Update the transaction status in database
                        transaction.Status = "CANCELLED";
                        transaction.UpdatedDate = DateTime.UtcNow;

                        _logger.LogInformation($"Successfully cancelled expired order: {orderCode}");
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid transaction ID format: {transaction.TransactionId}");
                    }
                }
                catch (PayOSError ex) when (ex.Code == "429")
                {
                    _logger.LogWarning("Rate limit reached. Pausing sync process.");
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error cancelling transaction {transaction.TransactionId}");
                }
            }

            // Save all changes to the database
            if (expiredTransactions.Any())
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Processed {expiredTransactions.Count} expired transactions");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing expired transactions");
        }
    }


    private async Task<int> GetLatestId()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var latestTransaction = await dbContext.PaymentTransactions
            .OrderByDescending(pt => pt.Id)
            .FirstOrDefaultAsync();

        return latestTransaction?.Id ?? 0;
    }

    private async Task<long?> GetTransactionIdById(AppDbContext dbContext, int id)
    {
        var transactionIdString = await dbContext.PaymentTransactions
            .Where(pt => pt.Id == id)
            .Select(pt => pt.TransactionId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(transactionIdString))
        {
            return null;
        }

        if (long.TryParse(transactionIdString, out long result))
        {
            return result;
        }

        _logger.LogWarning($"Invalid TransactionId format for Id {id}: {transactionIdString}");
        return null;
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
            existingTransaction.Status = paymentInfo.status;
            existingTransaction.UpdatedDate = DateTime.UtcNow;
            dbContext.Entry(existingTransaction).State = EntityState.Modified;
        }
    }
}
