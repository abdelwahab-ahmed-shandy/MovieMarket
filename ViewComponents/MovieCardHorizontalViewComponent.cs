namespace MovieMart.ViewComponents
{
    public class MovieCardHorizontalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Movie movie)
        {
            return View(movie);
        }
    }
}