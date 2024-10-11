using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Common.Payloads.Responses;
using BE_EXE201.Common;
using BE_EXE201.Dtos;
using BE_EXE201.Exceptions;
using BE_EXE201.Extensions.NewFolder;
using BE_EXE201.Helpers;
using BE_EXE201.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BE_EXE201.Entities;
using BE_EXE201.Repositories;
using BE_EXE201.Dtos.Payment;
using System.Text.Json;
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


        public PaymentController(IVnPayService vnPayService,
            UserService userService,
            IRepository<User, int> userRepository,
            AppDbContext dbContext,
            PaymentService paymentService)
        {
            _vnPayService = vnPayService;
            _userService = userService;
            _userRepository = userRepository;
            _paymentService = paymentService;
            _dbContext = dbContext; // Initialize the dbContext
        }

        // 1. Create Payment URL
        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] VnPaymentRequestModel paymentRequest)
        {
            try
            {
                var paymentUrl = await _paymentService.InitiateCheckoutAsync(HttpContext, paymentRequest);
                return Ok(new { PaymentUrl = paymentUrl });
            }
            catch (UserNotFoundException)
            {
                return NotFound("User not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }



        [HttpGet("CheckoutCallback")]
        public async Task<IActionResult> CheckoutCallback()
        {
            try
            {
                var message = await _paymentService.ProcessCheckoutCallbackAsync(HttpContext.Request.Query);
                return Ok(new { message });
            }
            catch (TransactionNotFoundException)
            {
                return NotFound("Transaction not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
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
