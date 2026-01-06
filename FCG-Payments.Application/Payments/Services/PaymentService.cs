using FCG.Shared.EventLog.Publisher;
using FCG.Shared.EventService.Contracts.Payment;
using FCG.Shared.EventService.Enums;
using FCG.Shared.EventService.Publisher;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Application.Shared.Results;
using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Domain.Payments.EventSourcing;
using Microsoft.Extensions.Configuration;

namespace FCG_Payments.Application.Payments.Services
{
    public class PaymentService(IRepository<Payment> repository,
                                IPaymentResolver resolver,
                                IEventServicePublisher servicePublisher,
                                IEventLogPublisher eventLogPublisher,
                                IConfiguration configuration) : IPaymentService
    {      
        public async Task<Result> PayAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            if (paymentId == Guid.Empty)
                return Result.Failure(new Error("400", "Código de pagamento inválido"));

            var payment = await repository.GetByIdAsync(paymentId, cancellationToken);

            if(payment is null)
                return Result.Failure(new Error("404","Pagamento não encontrado"));

            if(payment.Status == EPaymentStatus.Approved)
                return Result.Failure(new Error("400", "Este pagamento já foi efetuado."));

            var paymentStrategy = resolver.Resolve(payment.PaymentType);
            var success = await paymentStrategy.Pay(payment);

            if(!success)
            {
                var eventLogFailed = new PaymentProcessedEventLog(
                   payment.Id,
                   payment.ItemId,
                   payment.PaymentType,
                   EPaymentStatus.Failed);
                
                await eventLogPublisher.PublishAsync(eventLogFailed);

                var eventServiceFailed = new PaymentProcessedEvent(
                   payment.Id,
                   payment.ItemId,
                   payment.PaymentType,
                   EPaymentStatus.Failed);

                await servicePublisher.PublishAsync(eventServiceFailed, configuration["ServiceBus:Queues:PaymentsEvents"]!, "PaymentFailedEvent");

                return Result.Failure(new Error("402", "Pagamento recusado"));
            }

            var eventLogSuccess = new PaymentProcessedEventLog(
               payment.Id,
               payment.ItemId,
               payment.PaymentType,
               EPaymentStatus.Approved);

            await eventLogPublisher.PublishAsync(eventLogSuccess);

            var evtSucesso = new PaymentProcessedEvent(
                payment.Id,
                payment.ItemId,
                payment.PaymentType,
                EPaymentStatus.Approved);

            await servicePublisher.PublishAsync(evtSucesso, configuration["ServiceBus:Queues:PaymentsEvents"]!, "PaymentAprovedEvent");

            return Result.Success();
        }
    }
}
