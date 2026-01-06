using FCG.Shared.EventService.Consumer;
using FCG.Shared.EventService.Contracts.Library;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using System.Text.Json;

namespace FCG_Payments.Application.Payments.Handlers
{
    public class LibraryOrderHandler(IRepository<Payment> repository) : IMessageHandler
    {
        public string MessageType => "OrderCreated";

        public async Task HandleAsync(string message, CancellationToken cancellationToken)
        {
            LibraryOrderEvent libraryOrderEvent = JsonSerializer.Deserialize<LibraryOrderEvent>(message)!;

            var payment = Payment.Create(libraryOrderEvent.ItemId, libraryOrderEvent.UserId, libraryOrderEvent.GameId, libraryOrderEvent.PaymentType, libraryOrderEvent.PricePaid!.Value);

            await repository.AddAsync(payment, cancellationToken);
        }
    }
}
