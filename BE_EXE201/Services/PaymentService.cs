using BE_EXE201.Dtos.Payment;
using BE_EXE201.Entities;
using BE_EXE201.Exceptions;
using BE_EXE201.Extensions.NewFolder;
using BE_EXE201.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BE_EXE201.Services
{
    public class PaymentService
    {
        private UserService _userService;
        IRepository<PaymentTransaction, int> _paymentTransactionRepository;
        private readonly IVnPayService _vnPayService;
        private readonly AppDbContext _dbContext;


        public PaymentService(
            UserService userService,
           IRepository<PaymentTransaction, int> paymentTransactionRepository, 
            AppDbContext dbContext,
            IVnPayService vnPayService)
        {
            _userService = userService;
            _paymentTransactionRepository = paymentTransactionRepository;
            _vnPayService = vnPayService;
                _dbContext = dbContext; // Initialize the dbContext;
        }

        public async Task<string> InitiateCheckoutAsync(HttpContext httpContext, VnPaymentRequestModel paymentRequest)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _userService.GetUserById(paymentRequest.UserId);
                if (user == null) throw new UserNotFoundException();

                // Generate unique OrderId and TransactionId
                int orderId = GenerateUniqueOrderId();
                int transactionId = GenerateUniqueTransactionId();

                var paymentTransaction = new PaymentTransaction
                {
                    UserId = paymentRequest.UserId,
                    OrderId =  orderId.ToString(), // Keep it as int
                    Amount = paymentRequest.Amount,
                    Status = "Pending",
                    UpdatedDate = DateTime.Now,
                    TransactionId = transactionId.ToString() // Keep it as int
                };

                await _paymentTransactionRepository.AddAsync(paymentTransaction);
                await _paymentTransactionRepository.Commit();

                // Convert to string for payment request
                paymentRequest.OrderId = orderId.ToString();
                paymentRequest.Description = $"User {paymentRequest.UserId} paid {paymentRequest.Amount} to wallet on {DateTime.Now}";

                var paymentUrl = _vnPayService.CreatePaymentUrl(httpContext, paymentRequest);

                await transaction.CommitAsync();
                return paymentUrl;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // Example method to generate a unique OrderId
        private int GenerateUniqueOrderId()
        {
            // You can implement your logic here to generate a unique OrderId
            // For demonstration, we're using a random number generator
            Random random = new Random();
            return random.Next(10000000, 99999999); // Generate an 8-digit random number
        }

        // Example method to generate a unique TransactionId
        private int GenerateUniqueTransactionId()
        {
            // You can implement your logic here to generate a unique TransactionId
            // For demonstration, we're using a random number generator
            Random random = new Random();
            return random.Next(10000000, 99999999); // Generate an 8-digit random number
        }





        public async Task<string> ProcessCheckoutCallbackAsync(IQueryCollection collections)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Execute payment and get response
                var paymentResponse = _vnPayService.PaymentExecute(collections);

                // Validate payment response
                if (!paymentResponse.Success || paymentResponse.VnPayResponseCode != "00")
                {
                    return await HandleFailedTransaction(paymentResponse.OrderId, paymentResponse.VnPayResponseCode);
                }

                // Find the existing payment transaction
                var paymentTransaction = await _paymentTransactionRepository.FindByCondition(pt => pt.OrderId == paymentResponse.OrderId).FirstOrDefaultAsync();
                if (paymentTransaction == null) throw new TransactionNotFoundException();

                // Update payment transaction details
                paymentTransaction.Status = "Success";
                paymentTransaction.TransactionId = paymentResponse.TransactionId;

                // Retrieve the user based on OrderId and update their wallet
                var user = await _userService.GetUserByOrderId(paymentResponse.OrderId);
                if (user != null)
                {
                    await _userService.UpdateUserWallet(user, paymentResponse);
                    await _paymentTransactionRepository.Commit(); // Commit changes to payment transaction
                    await transaction.CommitAsync(); // Commit the transaction
                    return "Transaction successful and wallet updated.";
                }

                throw new UserNotFoundException();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback in case of error
                                                   // Log exception details (optional)
                Console.WriteLine($"Error processing payment: {ex.Message}");
                throw; // Rethrow exception to be handled by caller
            }
        }

        // Handle transaction failure logic
        private async Task<string> HandleFailedTransaction(string orderId, string responseCode)
        {
            var paymentTransaction = await _paymentTransactionRepository.FindByCondition(pt => pt.OrderId == orderId).FirstOrDefaultAsync();
            if (paymentTransaction != null)
            {
                paymentTransaction.Status = "Failed"; // Update transaction status
                await _paymentTransactionRepository.Commit(); // Commit changes to payment transaction
            }
            return $"Transaction failed. Response code: {responseCode}";
        }

    }

}
