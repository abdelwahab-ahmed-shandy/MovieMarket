namespace MovieMart.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderTotal { get; set; }
        public OrderStatus Status { get; set; }
        public bool PaymentStatus { get; set; }
        public bool OrderShipedStatus { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }

        public string? SessionId { get; set; }
        public string? PaymentStripeId { get; set; }

    }
}
