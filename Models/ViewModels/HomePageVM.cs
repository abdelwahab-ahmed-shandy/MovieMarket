namespace MovieMart.Models.ViewModels
{
    public class HomePageVM
    {
        public IEnumerable<Movie> FeaturedMovies { get; set; }
        public IEnumerable<Cinema> PopularCinemas { get; set; }
        public IEnumerable<Movie> NewReleases { get; set; }
        public IEnumerable<Movie> TopRated { get; set; }
        public IEnumerable<TvSeries> PopularSeries { get; set; }
        public IEnumerable<Category> MovieCategories { get; set; }
    }
}
