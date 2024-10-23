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

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); // Run every 1 minute
        }
    }

    private async Task SyncTransactions()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int latestId = await GetLatestId();
        int currentId = latestId;
        bool hasMoreTransactions = true;

        while (hasMoreTransactions)
        {
            int processedCount = 0;
            for (int i = 0; i < BatchSize; i++)
            {
                try
                {
                    long? transactionId = await GetTransactionIdById(dbContext, currentId);
                    if (!transactionId.HasValue)
                    {
                        hasMoreTransactions = false;
                        break;
                    }

                    var paymentInfo = await _payOS.getPaymentLinkInformation(transactionId.Value);

                    await SaveTransaction(dbContext, paymentInfo);
                    currentId--;
                    processedCount++;
                }
                catch (PayOSError ex) when (ex.Code == "429")
                {
                    _logger.LogWarning("Rate limit reached. Pausing sync process.");
                    await Task.Delay(TimeSpan.FromSeconds(30)); // Wait for 30 seconds before retrying
                    break;
                }
                catch (PayOSError ex) when (ex.Code == "14")
                {
                    // No new orders found, we can stop
                    hasMoreTransactions = false;
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing order with Id {currentId}");
                    currentId++; // Move to the next ID even if there's an error
                    continue;
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"Processed {processedCount} transactions. Last processed ID: {currentId - 1}");

            if (processedCount == 0)
            {
                hasMoreTransactions = false;
            }
        }

        _logger.LogInformation("Finished syncing all transactions");
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
