using FCG.Shared.EventService.Consumer;
using FCG.Shared.EventService.Contracts.Payment;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using System.Text.Json;

namespace FCG_Payments.Application.Payments.Handlers
{
    public class PaymentFailedHandler(IRepository<Payment> repository) : IMessageHandler
    {
        public string MessageType => "PaymentFailedEvent";

        public async Task HandleAsync(string message, CancellationToken cancellationToken)
        {
            PaymentProcessedEvent paymentFailedEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message)!;

            var payment = (await repository.GetByIdAsync(paymentFailedEvent.PaymentId, cancellationToken))!;

            payment.UpdateStatus(paymentFailedEvent.PaymentStatus);
            payment.UpdateLastDateChanged();

            await repository.UpdateAsync(payment, cancellationToken);
        }
    }
}
