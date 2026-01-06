namespace FCG_Payments.Domain.Payments.Exceptions.Payments
{
    public class ErrorMessage
    {
        public static PaymentErrorMessage Payment { get; } = new();
    }

    public class PaymentErrorMessage
    {
        public string OrderIdIsEmpty { get; } = "O número do pedido é obrigatório"; 
        public string InvalidPaymentType { get; } = "O tipo de pagamento é inválido";
        public string InvalidStatus { get; } = "O status do pagamento é inválido";
    }
}
