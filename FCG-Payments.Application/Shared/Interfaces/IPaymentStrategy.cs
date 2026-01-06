using FCG_Payments.Domain.Payments.Entities;

namespace FCG_Payments.Application.Shared.Interfaces
{
    public interface IPaymentStrategy
    {
        public Task<bool> Pay(Payment payment);
    }
}
