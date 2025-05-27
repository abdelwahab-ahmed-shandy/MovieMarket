namespace MovieMart.Models.ViewModels
{
    public class SearchResultVM
    {
        public List<TvSeries> TvSeries { get; set; } = new();
        public List<Season> Seasons { get; set; } = new();
        public List<Episode> Episodes { get; set; } = new();
        public List<Character> Characters { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Movie> Movies { get; set; } = new();
    }
}
