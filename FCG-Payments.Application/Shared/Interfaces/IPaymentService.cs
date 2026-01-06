using FCG_Payments.Application.Payments.Responses;
using FCG_Payments.Application.Shared.Results;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentService
    {       
        public Task<Result> PayAsync(Guid paymentId, CancellationToken cancellationToken = default);
    }
}
