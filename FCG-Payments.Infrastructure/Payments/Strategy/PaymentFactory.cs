using FCG.Shared.EventService.Enums;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Exceptions.Payments;

namespace FCG_Payments.Infrastructure.Payments.Strategy
{
    public class PaymentFactory : IPaymentResolver
    {
        private readonly Dictionary<EPaymentType, IPaymentStrategy> _strategies;

        public PaymentFactory()
        {
            _strategies = new Dictionary<EPaymentType, IPaymentStrategy>
            {
                { EPaymentType.Pix, new PixPayment() },
                { EPaymentType.CreditCard, new CreditCardPayment() },
                { EPaymentType.DebitCard, new DebitCardPayment()  },
                { EPaymentType.PayPal, new PaypalPayment() } 
            };
        }

        public IPaymentStrategy Resolve(EPaymentType type)
        {
            if (!_strategies.ContainsKey(type))
                throw new InvalidPaymentException("Tipo de pagamento não suportado");

            return _strategies[type];

        }
    }

}

