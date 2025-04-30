namespace MovieMart.ViewComponents
{
    public class MovieCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Movie movie)
        {
            return View(movie);
        }
    }
}