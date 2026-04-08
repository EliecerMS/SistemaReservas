namespace SistemaReservas.Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string Reference { get; set; } = string.Empty;

        // navigation
        public string PaymentMethodName { get; set; } = string.Empty;
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
