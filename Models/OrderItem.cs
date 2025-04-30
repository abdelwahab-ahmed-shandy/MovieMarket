namespace MovieMart.Models
{
    [PrimaryKey(nameof(OrderId), nameof(MovieId))]
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int Count { get; set; }
        public double Price { get; set; }
    }
}
