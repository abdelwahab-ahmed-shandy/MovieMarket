namespace MovieMart.Areas.Users.Views.Customer.Controllers
{
    [Area("Customer")]
    public class EpisodeController : Controller
    {
        private readonly IEpisodeRepository _episodeRepository;
        private readonly ISeasonRepository _seasonRepository;
        private readonly ILogger<EpisodeController> _logger;
        public EpisodeController(IEpisodeRepository episodeRepository, ISeasonRepository seasonRepository, ILogger<EpisodeController> logger)
        {
            this._episodeRepository = episodeRepository;
            this._seasonRepository = seasonRepository;
            this._logger = logger;
        }

        private IQueryable<Episode> ApplyFilters(IQueryable<Episode> query, EpisodeFilterVM filter)
        {
            // Apply season filter
            if (filter.SeasonId.HasValue && filter.SeasonId > 0)
            {
                query = query.Where(e => e.SeasonId == filter.SeasonId.Value);
            }

            // Apply TV series filter
            if (filter.TvSerieId.HasValue && filter.TvSerieId > 0)
            {
                query = query.Where(e => e.Season.TvSeriesId == filter.TvSerieId.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                query = query.Where(e =>
                    e.Title.Contains(filter.SearchQuery) ||
                    e.Description.Contains(filter.SearchQuery) ||
                    e.Season.TvSeries.Title.Contains(filter.SearchQuery));
            }

            return query;
        }

        private IQueryable<Episode> ApplySorting(IQueryable<Episode> query, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "title" => query.OrderBy(e => e.Title),
                "title_desc" => query.OrderByDescending(e => e.Title),
                "episode" => query.OrderBy(e => e.EpisodeNumber),
                "episode_desc" => query.OrderByDescending(e => e.EpisodeNumber),
                "duration" => query.OrderBy(e => e.Duration),
                "duration_desc" => query.OrderByDescending(e => e.Duration),
                "date" => query.OrderBy(e => e.Season.ReleaseYear),
                "date_desc" => query.OrderByDescending(e => e.Season.ReleaseYear),
                "rating" => query.OrderBy(e => e.Rating),
                "rating_desc" => query.OrderByDescending(e => e.Rating),
                _ => query.OrderBy(e => e.Season.TvSeries.Title)
                          .ThenBy(e => e.Season.SeasonNumber)
                          .ThenBy(e => e.EpisodeNumber)
            };
        }


        [HttpGet]
        public async Task<IActionResult> Index(EpisodeFilterVM filter)
        {
            // Base query with includes
            var query = _episodeRepository.Get()
                .Include(e => e.Season)
                    .ThenInclude(s => s.TvSeries)
                .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, filter);

            // Apply sorting
            query = ApplySorting(query, filter.SortBy);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var episodes = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Get filter options
            var seasons = await _seasonRepository.Get()
                .Include(s => s.TvSeries)
                .OrderBy(s => s.TvSeries.Title)
                .ThenBy(s => s.SeasonNumber)
                .ToListAsync();

            // Create view model
            var viewModel = new EpisodeIndexVM
            {
                Episodes = episodes,
                Seasons = seasons,
                Filter = filter,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AllEpisode(int id)
        {
            // Get episodes with season and TV series information
            var episodes = _episodeRepository.Get()
                .Where(e => e.SeasonId == id)
                .Include(e => e.Season)
                    .ThenInclude(s => s.TvSeries)
                .OrderBy(e => e.EpisodeNumber)  // Default ordering
                .ToList();

            // Get the season info for display
            var season = _seasonRepository.Get()
                .Include(s => s.TvSeries)
                .FirstOrDefault(s => s.Id == id);

            if (season == null)
            {
                return NotFound();
            }

            // Create view model
            var viewModel = new SeasonEpisodesVM
            {
                Episodes = episodes,
                Season = season,
                SeasonNumber = season.SeasonNumber,
                TvSeriesTitle = season.TvSeries.Title,
                ReleaseYear = season.ReleaseYear
            };

            return View(viewModel);
        }



    }
}

