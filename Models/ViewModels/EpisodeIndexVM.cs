namespace MovieMart.Models.ViewModels
{
    public class EpisodeIndexVM
    {
        public List<Episode> Episodes { get; set; } = new List<Episode>();
        public List<Season> Seasons { get; set; } = new List<Season>();
        public EpisodeFilterVM Filter { get; set; } = new EpisodeFilterVM();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
