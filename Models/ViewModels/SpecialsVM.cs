namespace MovieMart.Models.ViewModels
{
    public class SpecialsVM
    {
        public IEnumerable<Movie> MoviesWithSpecials { get; set; } = Enumerable.Empty<Movie>();
        public IEnumerable<Special> ActiveSpecials { get; set; } = Enumerable.Empty<Special>();

        public decimal CalculateDiscountedPrice(Movie movie)
        {
            if (movie == null) throw new ArgumentNullException(nameof(movie));

            var activeSpecial = GetActiveSpecial(movie);
            return activeSpecial == null
                ? (decimal)movie.Price
                : ApplyDiscount((decimal)movie.Price, activeSpecial.DiscountPercentage);
        }

        private Special? GetActiveSpecial(Movie movie)
        {
            return movie.MovieSpecials?
                .Where(ms => ms.Special != null)
                .FirstOrDefault(ms => IsSpecialActive(ms.Special!))?.Special;
        }

        private static bool IsSpecialActive(Special special)
        {
            var now = DateTime.Now;
            return special.StartDate <= now && special.EndDate >= now;
        }

        private static decimal ApplyDiscount(decimal price, decimal discount)
        {
            return discount switch
            {
                < 0 => throw new ArgumentException("Discount cannot be negative"),
                > 100 => throw new ArgumentException("Discount cannot exceed 100%"),
                _ => price * (100 - discount) / 100
            };
        }


    }
}
