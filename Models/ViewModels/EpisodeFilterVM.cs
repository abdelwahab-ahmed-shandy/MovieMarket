namespace MovieMart.Models.ViewModels
{
    public class EpisodeFilterVM
    {
        public int? SeasonId { get; set; }
        public int? TvSerieId { get; set; }
        public string SearchQuery { get; set; } = "";
        public string SortBy { get; set; } = "default";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
