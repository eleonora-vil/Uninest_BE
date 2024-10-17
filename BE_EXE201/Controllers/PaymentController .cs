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

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                // Prepare response with current user info
                var currentUserInfo = new
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
                    userInfo = currentUserInfo
                }));
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }
        }


        [HttpGet("getPayOSOrder/{orderCode}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderCode)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderCode);
                return Ok(new Response(0, "Ok", paymentLinkInformation));
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }

        }
        [HttpPut("cancelOrder/{orderCode}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderCode, string reason)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderCode, reason);
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

        [HttpPost("CheckOrderAndUpdateWallet")]
        public async Task<IActionResult> CheckOrderAndUpdateWallet([FromBody] int orderCode)
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

                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderCode);

                if (paymentLinkInformation.status == "PAID")
                {
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Update user's wallet
                            user.Wallet = (user.Wallet ?? 0) + paymentLinkInformation.amountPaid;
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

                            return Ok(new Response(0, "Wallet updated successfully", new
                            {
                                paymentInfo = paymentLinkInformation,
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
                else
                {
                    return Ok(new Response(0, "Payment not completed yet", new { paymentInfo = paymentLinkInformation }));
                }
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                return Ok(new Response(-1, "fail", null));
            }
        }
    }
}
  