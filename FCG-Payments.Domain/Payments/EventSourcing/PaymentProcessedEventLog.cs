using FCG.Shared.EventLog.Contracts;
using FCG.Shared.EventService.Enums;

namespace FCG_Payments.Domain.Payments.EventSourcing
{
    public class PaymentProcessedEventLog(Guid PaymentId, Guid OrderId, EPaymentType PaymentType, EPaymentStatus PaymentStatus) : EventLogMessage
    {
        public Guid PaymentId { get; private set; } = PaymentId;
        public Guid OrderId { get; private set; } = OrderId;
        public EPaymentType PaymentType { get; private set; } = PaymentType;
        public EPaymentStatus PaymentStatus { get; private set; } = PaymentStatus;
    }
}
