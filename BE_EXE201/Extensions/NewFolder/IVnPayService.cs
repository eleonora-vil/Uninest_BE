using BE_EXE201.Dtos;
using BE_EXE201.Dtos.Payment;

namespace BE_EXE201.Extensions.NewFolder
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
