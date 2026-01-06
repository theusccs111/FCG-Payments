using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;

namespace FCG_Payments.Infrastructure.Payments.Strategy
{
    public class CreditCardPayment : IPaymentStrategy
    {
        public async Task<bool> Pay(Payment payment)
        {
            Console.WriteLine($"Processando pagamento por {payment.PaymentType.ToString()}...");
            Console.WriteLine("Pagamento realizado");

            return true;
        }
    }
}
