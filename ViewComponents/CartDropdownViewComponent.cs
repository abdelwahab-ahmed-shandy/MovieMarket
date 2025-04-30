namespace MovieMart.ViewComponents
{
    public class CartDropdownViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartDropdownViewComponent(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("CartDropdown", new List<Cart>());
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var cartItems = await _cartService.GetCartItemsByUserIdAsync(user.Id);

            return View("CartDropdown", cartItems);
        }
    }
}