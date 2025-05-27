namespace MovieMart.Models.ViewModels
{
    public class TopRatedVM
    {
        public List<TvSeries> TopTvSeries { get; set; } = new List<TvSeries>();
        public List<Movie> TopMovies { get; set; } = new List<Movie>();
        public List<Episode> TopEpisodes { get; set; } = new List<Episode>();
    }
}
