using BE_EXE201.Exceptions;
using BE_EXE201.Extensions.NewFolder;
using BE_EXE201.Services;
using Microsoft.AspNetCore.Mvc;
using BE_EXE201.Entities;
using BE_EXE201.Repositories;
using BE_EXE201.Dtos.Payment;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BE_EXE201.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentController(PaymentService paymentService, IHttpContextAccessor httpContextAccessor)
        {
            _paymentService = paymentService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("CreatePayOSLink")]
        public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _paymentService.CreatePaymentLinkAsync(userEmail, body);
            return Ok(result);
        }

        [HttpGet("getPayOSOrder/{orderCode}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderCode)
        {
            var result = await _paymentService.GetOrderAsync(orderCode);
            return Ok(result);
        }

        [HttpPut("cancelOrder/{orderCode}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderCode, string reason)
        {
            var result = await _paymentService.CancelOrderAsync(orderCode, reason);
            return Ok(result);
        }

        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhook body)
        {
            var result = await _paymentService.ConfirmWebhookAsync(body.webhook_url);
            return Ok(result);
        }

        [HttpPost("payos_transfer_handler")]
        public IActionResult PayOSTransferHandler(WebhookType body)
        {
            var result = _paymentService.HandleTransferWebhook(body);
            return Ok(result);
        }

        [HttpPost("CheckOrderAndUpdateWallet")]
        public async Task<IActionResult> CheckOrderAndUpdateWallet([FromBody] CheckOrderRequest request)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _paymentService.CheckOrderAndUpdateWalletAsync(userEmail, request.OrderCode);
            return Ok(result);
        }
    }
}


//[HttpPost("ConfirmPurchase")]
//public async Task<IActionResult> ConfirmPurchase([FromBody] CheckOrderRequest request)
//{
//    int orderCode = request.OrderCode;
//    try
//    {
//        // Get the current user's email from the JWT token
//        var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
//        if (string.IsNullOrEmpty(userEmail))
//        {
//            return Unauthorized("User not authenticated");
//        }

//        // Find the user in the database
//        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
//        if (user == null)
//        {
//            return NotFound("User not found");
//        }

//        PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderCode);

//        if (paymentLinkInformation.status == "PAID")
//        {
//            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
//            {
//                try
//                {
//                    // Create a new purchase record
//                    var purchase = new Purchase
//                    {
//                        UserId = user.UserId,
//                        OrderCode = orderCode,
//                        Amount = paymentLinkInformation.amountPaid,
//                        PurchaseDate = DateTime.UtcNow,
//                        Status = "Completed"
//                    };

//                    _dbContext.Purchases.Add(purchase);
//                    await _dbContext.SaveChangesAsync();

//                    await transaction.CommitAsync();

//                    // Prepare response with purchase info
//                    var purchaseInfo = new
//                    {
//                        purchase.PurchaseId,
//                        purchase.OrderCode,
//                        purchase.Amount,
//                        purchase.PurchaseDate,
//                        purchase.Status
//                    };

//                    return Ok(new Response(0, "Purchase confirmed successfully", new
//                    {
//                        paymentInfo = paymentLinkInformation,
//                        purchaseInfo = purchaseInfo
//                    }));
//                }
//                catch (Exception ex)
//                {
//                    await transaction.RollbackAsync();
//                    _logger.LogError(ex, "Error confirming purchase");
//                    return StatusCode(500, new Response(-1, "An error occurred while confirming the purchase", null));
//                }
//            }
//        }
//        else
//        {
//            return Ok(new Response(0, "Payment not completed yet", new { paymentInfo = paymentLinkInformation }));
//        }
//    }
//    catch (Exception exception)
//    {
//        _logger.LogError(exception, "Error in ConfirmPurchase");
//        return StatusCode(500, new Response(-1, "An unexpected error occurred", null));
//    }
//}
