using FCG.Shared.EventService.Enums;
using FCG.Shared.Transactional;
using FCG_Payments.Domain.Payments.Exceptions.Payments;

namespace FCG_Payments.Domain.Payments.Entities
{
    public class Payment : Entity
    {
        #region Properties

        public Guid ItemId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public EPaymentType PaymentType { get; private set; }
        public EPaymentStatus Status { get; private set; }
        public decimal Price { get; private set; }

        #endregion

        #region Constructors
        private Payment(Guid id, Guid itemId, Guid userId, Guid gameId, EPaymentType paymentType, EPaymentStatus status, decimal price)
            : base(id)
        {
            ItemId = itemId;
            UserId = userId;
            GameId = gameId;
            Price = price;
            PaymentType = paymentType;
            Status = status;
        }
        #endregion

        #region Factory Methods
        public static Payment Create(Guid itemId, Guid userId, Guid gameId, EPaymentType paymentType, decimal price)
        {
            if (itemId == Guid.Empty)
                throw new ItemIdEmptyException(ErrorMessage.Payment.OrderIdIsEmpty);

            if (userId == Guid.Empty)
                throw new UserIdEmptyException(ErrorMessage.Payment.OrderIdIsEmpty);

            if (gameId == Guid.Empty)
                throw new GameIdEmptyException(ErrorMessage.Payment.OrderIdIsEmpty);

            if (price <= 0)
                throw new PriceInvalidException(ErrorMessage.Payment.OrderIdIsEmpty);

            if (!Enum.IsDefined(typeof(EPaymentType), paymentType))
                throw new InvalidPaymentException(ErrorMessage.Payment.InvalidPaymentType);

            return new Payment(Guid.NewGuid(), itemId, userId, gameId, paymentType, EPaymentStatus.Pending, price);
        }
        #endregion
               

        #region Methods
        public void UpdateStatus(EPaymentStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(EPaymentStatus), newStatus))
                throw new InvalidStatusException(ErrorMessage.Payment.InvalidStatus);
            Status = newStatus;
            UpdateLastDateChanged();
        }        

        #endregion

    }
}
