using FCG.Shared.EventService.Consumer;
using FCG.Shared.EventService.Contracts.Game;
using FCG.Shared.EventService.Contracts.Library;
using FCG.Shared.EventService.Contracts.Payment;
using FCG.Shared.EventService.Enums;
using FCG.Shared.EventService.Publisher;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FCG_Payments.Application.Payments.Handlers
{
    public class PaymentAprovedHandler(
        IRepository<Payment> repository,
        IEventServicePublisher servicePublisher,
        IConfiguration configuration) : IMessageHandler
    {
        public string MessageType => "PaymentAprovedEvent";

        public async Task HandleAsync(string message, CancellationToken cancellationToken)
        {
            PaymentProcessedEvent paymentAprovedEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message)!;

            var payment = await repository.GetByIdAsync(paymentAprovedEvent.PaymentId, cancellationToken);
            if (payment is null)
                throw new Exception($"Payment with id {paymentAprovedEvent.PaymentId} was not found");

            payment.UpdateStatus(paymentAprovedEvent.PaymentStatus);
            payment.UpdateLastDateChanged();
            
            await repository.UpdateAsync(payment, cancellationToken);

            var libraryItemEvent = new LibraryItemCreatedEvent(payment.ItemId, payment.UserId, payment.GameId, EOrderStatus.Owned, payment.Price, payment.PaymentType);
            await servicePublisher.PublishAsync(libraryItemEvent, configuration["ServiceBus:Queues:LibrariesEvents"]!, "LibraryItemCreated");

            var gameSoldEvent = new GameSoldEvent(payment.GameId);
            await servicePublisher.PublishAsync(gameSoldEvent, configuration["ServiceBus:Queues:GamesEvents"]!, "GameSold");
        }
    }
}
