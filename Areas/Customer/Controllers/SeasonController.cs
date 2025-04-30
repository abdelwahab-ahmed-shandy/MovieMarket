using Microsoft.AspNetCore.Mvc;
using MovieMart.Repositories;
using MovieMart.Repositories.IRepositories;

namespace MovieMart.Areas.Users.Views.Customer.Controllers
{
    [Area("Customer")]
    public class SeasonController : Controller
    {
        private readonly ISeasonRepository seasonRepository;
        private readonly ITvSeriesRepository tvSeriesRepository;
        public SeasonController(ISeasonRepository seasonRepository, ITvSeriesRepository tvSeriesRepository)
        {
            this.seasonRepository = seasonRepository;
            this.tvSeriesRepository = tvSeriesRepository;
        }
        public IActionResult Index()
        {
            var Season = seasonRepository.Get()
                .Include(S => S.TvSeries);
            return View(Season.ToList());
        }

        [HttpGet]
        public IActionResult AllSeasons(int id)
        {
            var seasons = seasonRepository.Get()
                .Where(s => s.TvSeriesId == id)
                .Include(s => s.TvSeries)
                .Include(s => s.Episodes)
                .ToList();

            if (!seasons.Any())
            {
                // Get TV series info even if no seasons exist
                var tvSeries = tvSeriesRepository.Get()
                    .FirstOrDefault(t => t.Id == id);

                if (tvSeries == null)
                {
                    return NotFound();
                }

                ViewData["SeriesTitle"] = tvSeries.Title;
            }

            return View(seasons);
        }


    }
}
