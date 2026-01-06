using FCG.Shared.EventService.Enums;

namespace FCG_Payments.Application.Payments.Responses
{
    public sealed record PaymentResponse(Guid Id, Guid OrderId, EPaymentType PaymentType, EPaymentStatus Status);
}
