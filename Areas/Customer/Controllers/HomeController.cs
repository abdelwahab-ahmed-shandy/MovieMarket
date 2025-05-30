using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieMart.Models;
using MovieMart.Repositories.IRepositories;

namespace MovieMart.Areas.Users.Views.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly ITvSeriesRepository _tvSeriesRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEpisodeRepository _episodeRepository;
        private readonly ISeasonRepository _seasonRepository;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IMovieRepository movieRepository,
                                ICinemaRepository cinemaRepository,
                                    ITvSeriesRepository tvSeriesRepository,
                                        ICategoryRepository categoryRepository,
                                         ICharacterRepository characterRepository,
                                            IEpisodeRepository episodeRepository,
                                            ISeasonRepository seasonRepository,
                                        ILogger<HomeController> logger)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _cinemaRepository = cinemaRepository ?? throw new ArgumentNullException(nameof(cinemaRepository));
            _tvSeriesRepository = tvSeriesRepository ?? throw new ArgumentNullException(nameof(tvSeriesRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
            _episodeRepository = episodeRepository ?? throw new ArgumentNullException(nameof(episodeRepository));
            _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
            _logger = logger;
        }



        #region index Customer Home Page

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new HomePageVM
            {
                FeaturedMovies = _movieRepository.Get()
                    .Include(m => m.Category)
                    .Include(c => c.CinemaMovies)
                    .OrderByDescending(m => m.ReleaseYear)
                    .Take(8)
                    .ToList(),

                PopularCinemas = _cinemaRepository.Get()
                    .OrderByDescending(c => c.Name)
                    .Take(6)
                    .ToList(),

                NewReleases = _movieRepository.Get()
                    .Include(m => m.Category)
                    .OrderByDescending(m => m.ReleaseYear)
                    .Take(6)
                    .ToList(),

                TopRated = _movieRepository.Get()
                    .Include(m => m.Category)
                    .OrderByDescending(m => m.Rating)
                    .Take(6)
                    .ToList(),

                PopularSeries = _tvSeriesRepository.Get()
                    .Include(s => s.Seasons)
                    .OrderByDescending(s => s.ReleaseYear)
                    .Take(6)
                    .ToList(),

                MovieCategories = _categoryRepository.Get()
                    .Take(8)
                    .ToList()
            };

            return View(viewModel);
        }

        #endregion



        #region As Search

        public async Task<IActionResult> GlobalSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(nameof(GlobalSearch));

            var tvSeries = await _tvSeriesRepository.Get()
                .Where(s => EF.Functions.Like(s.Title, $"%{query}%"))
                .ToListAsync();

            var seasons = await _seasonRepository.Get()
                .Where(s => EF.Functions.Like(s.Title, $"%{query}%"))
                .ToListAsync();

            var episodes = await _episodeRepository.Get()
                .Where(e => EF.Functions.Like(e.Title, $"%{query}%"))
                .ToListAsync();

            var characters = await _characterRepository.Get()
                .Where(c => EF.Functions.Like(c.Name, $"%{query}%"))
                .ToListAsync();

            var categories = await _categoryRepository.Get()
                .Where(cat => EF.Functions.Like(cat.Name, $"%{query}%"))
                .ToListAsync();

            var movies = await _movieRepository.Get()
                .Where(m => EF.Functions.Like(m.Title, $"%{query}%"))
                .ToListAsync();

            var result = new SearchResultVM
            {
                TvSeries = tvSeries,
                Seasons = seasons,
                Episodes = episodes,
                Characters = characters,
                Categories = categories,
                Movies = movies
            };

            ViewBag.Query = query;

            return View(nameof(GlobalSearch), result);
        }



        #endregion



        #region Specials and New Releases and Top Rated 

        [HttpGet]
        public async Task<IActionResult> Specials()
        {
            try
            {
                var currentDate = DateTime.Now;

                var specialsData = await _movieRepository.Get()
                    .AsNoTracking()
                    .Include(m => m.MovieSpecials)
                        .ThenInclude(ms => ms.Special)
                    .Include(m => m.Category)
                    .Select(m => new
                    {
                        Movie = m,
                        ActiveSpecials = m.MovieSpecials
                            .Where(ms => ms.Special != null &&
                                        ms.Special.StartDate <= currentDate &&
                                        ms.Special.EndDate >= currentDate)
                            .Select(ms => ms.Special)
                            .ToList()
                    })
                    .ToListAsync();

                var activeSpecials = specialsData
                    .SelectMany(x => x.ActiveSpecials)
                    .Distinct()
                    .ToList();

                var moviesWithSpecials = specialsData
                    .Where(x => x.ActiveSpecials.Any())
                    .Select(x => x.Movie)
                    .ToList();

                var viewModel = new SpecialsVM
                {
                    MoviesWithSpecials = moviesWithSpecials,
                    ActiveSpecials = activeSpecials
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading special offers");
                return View("Error");
            }
        }
        private static List<Movie> GetMoviesWithActiveSpecials(IQueryable<Movie> query, DateTime currentDate)
        {
            return query
                .AsEnumerable()
                .Where(m => m.MovieSpecials?
                    .Any(ms => IsSpecialActive(ms.Special, currentDate)) ?? false)
                .ToList();
        }

        private static List<Special> GetActiveSpecials(IQueryable<Movie> query, DateTime currentDate)
        {
            return query
                .SelectMany(m => m.MovieSpecials
                    .Where(ms => IsSpecialActive(ms.Special, currentDate))
                    .Select(ms => ms.Special))
                .Distinct()
                .ToList();
        }

        private static bool IsSpecialActive(Special? special, DateTime currentDate)
        {
            return special != null &&
                   special.StartDate <= currentDate &&
                   special.EndDate >= currentDate;
        }



        [HttpGet]
        public async Task<IActionResult> NewReleases()
        {
            try
            {
                var newReleases = await _movieRepository.Get()
                    .AsNoTracking()
                    .Where(m => m.StartDate >= DateTime.Now.AddMonths(-3))
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenByDescending(m => m.Rating)
                    .Take(20)
                    .Include(m => m.Category)
                    .ToListAsync();

                return View(newReleases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading new releases");
                return View("Error");
            }
        }


        [ResponseCache(Duration = 3600)]
        [HttpGet]
        public async Task<IActionResult> TopRated()
        {
            // Get top rated TV series (assuming you have a Rating property)
            var topTvSeries = await _tvSeriesRepository.Get()
                .OrderByDescending(s => s.Rating)
                .Take(10)
                .ToListAsync();

            // Get top rated movies
            var topMovies = await _movieRepository.Get()
                .OrderByDescending(m => m.Rating)
                .Take(10)
                .ToListAsync();

            // Get top rated episodes
            var topEpisodes = await _episodeRepository.Get()
                .Include(e => e.Season)
                .ThenInclude(s => s.TvSeries)
                .OrderByDescending(e => e.Rating)
                .Take(10)
                .ToListAsync();

            var model = new TopRatedVM
            {
                TopTvSeries = topTvSeries,
                TopMovies = topMovies,
                TopEpisodes = topEpisodes
            };

            return View(model);
        }


        #endregion





    }

}
