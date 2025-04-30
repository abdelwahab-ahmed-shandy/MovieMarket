using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(IMovieRepository movieRepository,
                                ICinemaRepository cinemaRepository,
                                    ITvSeriesRepository tvSeriesRepository,
                                        ICategoryRepository categoryRepository)
        {
            _movieRepository = movieRepository;
            _cinemaRepository = cinemaRepository;
            _tvSeriesRepository = tvSeriesRepository;
            _categoryRepository = categoryRepository;
        }

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

    }

}
