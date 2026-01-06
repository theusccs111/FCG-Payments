using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;

namespace FCG_Payments.Infrastructure.Payments.Strategy
{
    public class PaypalPayment : IPaymentStrategy
    {
        public async Task<bool> Pay(Payment payment)
        {
            Console.WriteLine($"Processando pagamento por {payment.PaymentType.ToString()}...");
            Console.WriteLine("Houve um erro. Pagamento não realizado.");

            return true;
        }
    }
}
