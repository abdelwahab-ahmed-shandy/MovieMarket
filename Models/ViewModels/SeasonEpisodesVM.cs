namespace MovieMart.Models.ViewModels
{
    public class SeasonEpisodesVM
    {
        public List<Episode> Episodes { get; set; }
        public Season Season { get; set; }
        public int SeasonNumber { get; set; }
        public string TvSeriesTitle { get; set; }
        public int ReleaseYear { get; set; }
    }
}
