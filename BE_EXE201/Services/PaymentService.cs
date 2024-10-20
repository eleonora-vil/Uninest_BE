using BE_EXE201.Dtos.Payment;
using BE_EXE201.Entities;
using BE_EXE201.Exceptions;
using BE_EXE201.Extensions.NewFolder;
using BE_EXE201.Repositories;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;

namespace BE_EXE201.Services
{
    public class PaymentService 
    {
        private readonly IRepository<User, int> _userRepository;
        private readonly IRepository<PaymentTransaction, string> _paymentTransactionRepository;
        private readonly PayOS _payOS;

        public PaymentService(
            IRepository<User, int> userRepository,
            IRepository<PaymentTransaction, string> paymentTransactionRepository,
            PayOS payOS)
        {
            _userRepository = userRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _payOS = payOS;
        }

        public async Task<Response> CreatePaymentLinkAsync(string userEmail, CreatePaymentLinkRequest body)
        {
            var user = _userRepository.FindByCondition(u => u.Email == userEmail).FirstOrDefault();
            if (user == null)
            {
                return new Response(-1, "User not found", null);
            }

            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            var items = new List<ItemData> { new ItemData(body.productName, 1, body.price) };
            string buyerName = !string.IsNullOrEmpty(body.buyerName) ? body.buyerName : user.FullName;

            var paymentData = new PaymentData(
                orderCode,
                body.price,
                body.description,
                items,
                body.cancelUrl,
                body.returnUrl,
                buyerName
            );

            var createPayment = await _payOS.createPaymentLink(paymentData);

            var paymentTransaction = new PaymentTransaction
            {
                TransactionId = createPayment.orderCode.ToString(),
                UserId = user.UserId,
                Amount = body.price,
                Status = "PENDING",
                CreateDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _paymentTransactionRepository.AddAsync(paymentTransaction);
            await _paymentTransactionRepository.Commit();

            var currentUserInfo = new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Wallet
            };

            return new Response(0, "success", new { paymentInfo = createPayment, userInfo = currentUserInfo });
        }

        public async Task<Response> GetOrderAsync(int orderCode)
        {
            var paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderCode);
            return new Response(0, "Ok", paymentLinkInformation);
        }

        public async Task<Response> CheckOrderAndUpdateWalletAsync(string userEmail, int orderCode)
        {
            var user = _userRepository.FindByCondition(u => u.Email == userEmail).FirstOrDefault();
            if (user == null)
            {
                return new Response(-1, "User not found", null);
            }

            var paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderCode);

            if (paymentLinkInformation.status == "PAID")
            {
                user.Wallet = (user.Wallet ?? 0) + paymentLinkInformation.amountPaid;
                _userRepository.Update(user);
                await _userRepository.Commit();

                var updatedUserInfo = new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Wallet
                };

                return new Response(0, "Wallet updated successfully", new { paymentInfo = paymentLinkInformation, userInfo = updatedUserInfo });
            }

            return new Response(0, "Payment not completed yet", new { paymentInfo = paymentLinkInformation });
        }

        public async Task<Response> CancelOrderAsync(int orderCode, string reason)
        {
            var result = await _payOS.cancelPaymentLink(orderCode, reason);
            return new Response(0, "Order canceled", result);
        }

        public async Task<Response> ConfirmWebhookAsync(string webhookUrl)
        {
            await _payOS.confirmWebhook(webhookUrl);
            return new Response(0, "Webhook confirmed", null);
        }

        public Response HandleTransferWebhook(WebhookType webhookData)
        {
            var data = _payOS.verifyPaymentWebhookData(webhookData);

            if (data.description == "Ma giao dich thu nghiem" || data.description == "VQRIO123")
            {
                return new Response(0, "Transfer ignored for test transaction", null);
            }

            return new Response(0, "Transfer handled", null);
        }
    }
}
