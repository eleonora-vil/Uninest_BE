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
        private readonly IVnPayService _vnPayService;
        private readonly UserService _userService; // Assume it handles wallet updates
        private readonly IRepository<User, int> _userRepository;
        private readonly PaymentService _paymentService;
        private readonly AppDbContext _dbContext;
        private readonly PayOS _payOS;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public PaymentController(IVnPayService vnPayService,
            UserService userService,
            IRepository<User, int> userRepository,
            AppDbContext dbContext,
            PaymentService paymentService, PayOS payOS, IHttpContextAccessor httpContextAccessor)
        {
            _vnPayService = vnPayService;
            _userService = userService;
            _userRepository = userRepository;
            _paymentService = paymentService;
            _dbContext = dbContext; // Initialize the dbContext
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("CreatePayOSLink")]
        public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        {
            try
            {
                // Get the current user's email from the JWT token
                var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("User not authenticated");
                }

                // Find the user in the database
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

                ItemData item = new ItemData(body.productName, 1, body.price);
                List<ItemData> items = new List<ItemData>();
                items.Add(item);

                string buyerName = !string.IsNullOrEmpty(body.buyerName) ? body.buyerName : user.FullName;

                PaymentData paymentData = new PaymentData(
                    orderCode,
                    body.price,
                    body.description,
                    items,
                    body.cancelUrl,
                    body.returnUrl,
                    buyerName
                    );
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                        // Update user's wallet
                        user.Wallet = (user.Wallet ?? 0) + body.price;
                        _dbContext.Entry(user).Property(u => u.Wallet).IsModified = true;

                        await _dbContext.SaveChangesAsync();

                        await transaction.CommitAsync();

                        // Prepare response with updated user info
                        var updatedUserInfo = new
                        {
                            user.UserId,
                            user.FullName,
                            user.Email,
                            user.PhoneNumber,
                            user.Wallet
                        };

                        return Ok(new Response(0, "success", new
                        {
                            paymentInfo = createPayment,
                            userInfo = updatedUserInfo
                        }));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }
        }

        [HttpGet("getPayOSOrder/{orderId}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderId);
                return Ok(new Response(0, "Ok", paymentLinkInformation));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }

        }
        [HttpPut("cancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderId, string reason)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderId, reason);
                return Ok(new Response(0, "Ok", paymentLinkInformation));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }

        }
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhook body)
        {
            try
            {
                await _payOS.confirmWebhook(body.webhook_url);
                return Ok(new Response(0, "Ok", null));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }
        }

        [HttpPost("payos_transfer_handler")]
        public IActionResult payOSTransferHandler(WebhookType body)
        {
            try
            {
                WebhookData data = _payOS.verifyPaymentWebhookData(body);

                if (data.description == "Ma giao dich thu nghiem" || data.description == "VQRIO123")
                {
                    return Ok(new Response(0, "Ok", null));
                }
                return Ok(new Response(0, "Ok", null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok(new Response(-1, "fail", null));
            }
        }
        //[HttpPost("/create-payment-link")]
        //public async Task<IActionResult> Checkout()
        //{
        //    try
        //    {
        //        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        //        ItemData item = new ItemData("Mì tôm hảo hảo ly", 1, 1000);
        //        List<ItemData> items = new List<ItemData>();
        //        items.Add(item);
        //        PaymentData paymentData = new PaymentData(orderCode, 1000, "Thanh toan don hang", items, "https://localhost:3002/cancel", "https://localhost:3002/success");

        //        CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

        //        return Redirect(createPayment.checkoutUrl);
        //    }
        //    catch (System.Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //        return Redirect("https://localhost:3002/");
        //    }
        //}

    }
}

// 2. VNPay Callback to confirm transaction
/* [HttpGet("PaymentCallback")]
 public async Task<IActionResult> PaymentCallback([FromQuery] IQueryCollection collections)
 {
     try
     {
         // Execute the payment and validate the signature
         var paymentResponse = _vnPayService.PaymentExecute(collections);

         if (paymentResponse.Success)
         {
             // Log success details for debugging
             // You can log paymentResponse details here

             // Retrieve user based on the order ID from the response
             var user = await _userService.GetUserByOrderId(paymentResponse.OrderId);
             if (user != null)
             {
                 // Update the user's wallet based on the successful payment
                 await _userService.UpdateUserWallet(user, paymentResponse);
                 return Ok(new { message = "Transaction successful and wallet updated." });
             }
             else
             {
                 // User not found
                 return NotFound("User not found.");
             }
         }
         else
         {
             // Log transaction failure details for debugging
             return BadRequest(new { message = "Transaction failed." });
         }
     }
     catch (Exception ex)
     {
         // Log detailed error for troubleshooting
         // For example, you might want to use a logging framework
         return StatusCode(500, new { message = "Internal server error", details = ex.Message });
     }
 }*/

//// 1. Create Payment URL
//[HttpPost("Checkout")]
//public async Task<IActionResult> Checkout([FromBody] VnPaymentRequestModel paymentRequest)
//{
//    try
//    {
//        var paymentUrl = await _paymentService.InitiateCheckoutAsync(HttpContext, paymentRequest);
//        return Ok(new { PaymentUrl = paymentUrl });
//    }
//    catch (UserNotFoundException)
//    {
//        return NotFound("User not found.");
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, new { message = "Internal server error", details = ex.Message });
//    }
//}



//[HttpGet("CheckoutCallback")]
//public async Task<IActionResult> CheckoutCallback()
//{
//    try
//    {
//        var message = await _paymentService.ProcessCheckoutCallbackAsync(HttpContext.Request.Query);
//        return Ok(new { message });
//    }
//    catch (TransactionNotFoundException)
//    {
//        return NotFound("Transaction not found.");
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, new { message = "Internal server error", details = ex.Message });
//    }
//}
