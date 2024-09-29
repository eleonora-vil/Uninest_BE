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

namespace BE_EXE201.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly UserService _userService; // Assume it handles wallet updates
        private readonly IRepository<User, int> _userRepository;

        public PaymentController(IVnPayService vnPayService, UserService userService, IRepository<User, int> userRepository)
        {
            _vnPayService = vnPayService;
            _userService = userService;
            _userRepository = userRepository;
        }

        // 1. Create Payment URL
        [HttpPost("CreatePayment")]
        public IActionResult CreatePayment([FromBody] VnPaymentRequestModel paymentRequest)
        {
            // Use the provided UserId and Amount, and auto-generate the description
            paymentRequest.Description = $"User {paymentRequest.UserId} paid {paymentRequest.Amount} to wallet on {DateTime.Now}";

            var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, paymentRequest);
            return Ok(new { PaymentUrl = paymentUrl });
        }

        // 2. VNPay Callback to confirm transaction
        [HttpGet("PaymentCallback")]
        public async Task<IActionResult> PaymentCallback([FromQuery] IQueryCollection collections)
        {
            var paymentResponse = _vnPayService.PaymentExecute(collections);

            if (paymentResponse.Success)
            {
                // Fetch user using the OrderId and update their wallet
                var user = await _userService.GetUserByOrderId(paymentResponse.OrderId);

                if (user is not null)
                {
                    await _userService.UpdateUserWallet(user, paymentResponse);
                    return Ok(new { message = "Transaction successful and wallet updated." });
                }
                else
                {
                    return NotFound("User not found.");
                }
            }
            else
            {
                return BadRequest(new { message = "Transaction failed." });
            }
        }
    }
}
