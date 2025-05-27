using Microsoft.AspNetCore.Mvc;
using MovieMart.Repositories.IRepositories;
using MovieMart.Repositories;
using MovieMart.Repositories.IRepositories;

namespace MovieMarket.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CinemaController : Controller
    {
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IMovieRepository _movieRepository;

        public CinemaController(ICinemaRepository cinemaRepository, IMovieRepository movieRepository)
        {
            this._cinemaRepository = cinemaRepository;
            this._movieRepository = movieRepository;
        }



        public IActionResult Index()
        {
            var ViewCinema = _cinemaRepository.Get().ToList();

            return View(ViewCinema);
        }


        public IActionResult MoreMovieWithCinema(int Id, string category = null, string timeFilter = null, string sort = null)
        {
            var cinemaWithMovies = _cinemaRepository.Get()
                .Where(c => c.Id == Id)
                .Include(c => c.CinemaMovies)
                .ThenInclude(cm => cm.Movie)
                .ThenInclude(m => m.Category)
                .FirstOrDefault();

            if (cinemaWithMovies == null || !cinemaWithMovies.CinemaMovies.Any())
            {
                return NotFound();
            }

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category) && category != "All Categories")
            {
                cinemaWithMovies.CinemaMovies = cinemaWithMovies.CinemaMovies
                    .Where(cm => cm.Movie.Category.Name == category)
                    .ToList();
            }

            // Apply time filter if provided
            if (!string.IsNullOrEmpty(timeFilter) && timeFilter != "All Times")
            {
                cinemaWithMovies.CinemaMovies = cinemaWithMovies.CinemaMovies.Where(cm =>
                {
                    var hour = cm.ShowTime.Hour;
                    return timeFilter switch
                    {
                        "Morning (Before 12 PM)" => hour < 12,
                        "Afternoon (12-5 PM)" => hour >= 12 && hour < 17,
                        "Evening (After 5 PM)" => hour >= 17,
                        _ => true
                    };
                }).ToList();
            }

            // Apply sorting
            cinemaWithMovies.CinemaMovies = sort switch
            {
                "Most Popular" => cinemaWithMovies.CinemaMovies.OrderByDescending(cm => cm.Movie.Rating).ToList(),
                "Newest First" => cinemaWithMovies.CinemaMovies.OrderByDescending(cm => cm.Movie.ReleaseYear).ToList(),
                "Price: Low to High" => cinemaWithMovies.CinemaMovies.OrderBy(cm => cm.Movie.Price).ToList(),
                _ => cinemaWithMovies.CinemaMovies.ToList()
            };

            // Get distinct categories for dropdown
            ViewBag.Categories = cinemaWithMovies.CinemaMovies
                .Select(cm => cm.Movie.Category.Name)
                .Distinct()
                .ToList();

            // Pass current filter values to view
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentTimeFilter = timeFilter;
            ViewBag.CurrentSort = sort;

            if (cinemaWithMovies?.CinemaMovies != null)
            {
                ViewBag.Count = cinemaWithMovies.CinemaMovies.Count;
            }
            else
            {
                ViewBag.Count = 0;
            }

            return View(cinemaWithMovies);
        }



    }
}
