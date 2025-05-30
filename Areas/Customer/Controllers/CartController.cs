using Microsoft.AspNetCore.Authorization;
using Stripe.Checkout;

namespace MovieMart.Areas.Users.Views.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        public CartController(UserManager<ApplicationUser> userManager, ICartRepository cartRepository,
                                    IMovieRepository movieRepository, ICinemaRepository cinemaRepository
                                    , IOrderRepository orderRepository, IOrderItemRepository orderItemRepository)
        {
            this._userManager = userManager;
            this._cartRepository = cartRepository;
            this._cinemaRepository = cinemaRepository;
            this._movieRepository = movieRepository;
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
        }

        #region View Cart
        public IActionResult Index()
        {
            var appUserId = _userManager.GetUserId(User);

            var cart = _cartRepository.Get(
                e => e.ApplicationUserId == appUserId && e.CartStatus == CartStatus.InCart,
                includes: [e => e.Movie, e => e.Cinema, e => e.ApplicationUser]
            );

            ViewBag.Total = cart.Sum(e => e.Movie.Price * e.Count);

            return View(cart.ToList());
        }
        #endregion


        #region Add And Delete Cart
        public IActionResult AddToCart(int MovieId, int CinemaId, int count)
        {
            var appUserId = _userManager.GetUserId(User);


            //todo : Error in The Valadate ...

            /* Validate the entered data
             If the user ID is empty (meaning the user is not logged in),
             If the movie ID (MovieId) or cinema ID (CinemaId) is zero or less (invalid information), or
             If the number of tickets (count) is less than or equal to zero (invalid information),
             A BadRequest status will be returned to indicate that there is an error in the entered data */
            //if (string.IsNullOrEmpty(appUserId) || MovieId <= 0 || CinemaId <= 0 || count <= 0)
            //{
            //    TempData["ErrorMessage"] = "Some of the input data is invalid.";
            //    return RedirectToAction("ErrorPage");  // أو أي صفحة تعرض الأخطاء
            //}

            Cart cart = new Cart()
            {
                ApplicationUserId = appUserId,
                MovieId = MovieId,
                CinemaId = CinemaId,
                Count = count,
                CartStatus = CartStatus.InCart
            };

            var cartDB = _cartRepository.GetOne(e => e.ApplicationUserId == appUserId &&
                                                 e.MovieId == MovieId && e.CinemaId == CinemaId);

            if (cartDB != null)
                cartDB.Count = cartDB.Count + count;
            else
                _cartRepository.Create(cart);

            if (cartDB != null)
            {
                cartDB.Count += count;
            }
            else
            {
                _cartRepository.Create(cart);
            }

            _cartRepository.SaveDB();

            TempData["notification"] = "Add Movie To Cart successfully!";
            TempData["MessageType"] = "Success";

            return RedirectToAction("Index", "Cart", new { area = "Customer" });
        }

        public IActionResult DeleteOnCart(int movieId, int cinemaId)
        {
            var appUserId = _userManager.GetUserId(User);

            var DeleteItem = _cartRepository.Get(
                filter: e => e.ApplicationUserId == appUserId
                             && e.MovieId == movieId
                             && e.CinemaId == cinemaId
            ).FirstOrDefault();


            if (DeleteItem != null)
            {
                _cartRepository.Delete(DeleteItem);
                _cartRepository.SaveDB();

                TempData["notification"] = "🗑️ The item has been removed from your cart.";
                TempData["MessageType"] = "Success";
            }
            else
            {
                TempData["notification"] = "⚠️ The item you're trying to remove does not exist.";
                TempData["MessageType"] = "Warning";
            }

            return RedirectToAction("Index");
        }


        #endregion


        #region Increment And Decrement

        public IActionResult Increment(int movieId)
        {
            var appUserId = _userManager.GetUserId(User);

            var cartItem = _cartRepository.GetOne(e => e.ApplicationUserId == appUserId && e.MovieId == movieId); // مثال لتحميل العنصر من قاعدة البيانات
            if (cartItem != null)
            {
                cartItem.Count += 1;
                _cartRepository.SaveDB();
            }

            TempData["notification"] = " One More Ticket Added To Your Cart !";
            TempData["MessageType"] = "Success";
            return RedirectToAction("Index");
        }


        public IActionResult Decrement(int movieId)
        {
            var appUserId = _userManager.GetUserId(User);

            var cartItem = _cartRepository.GetOne(e => e.ApplicationUserId == appUserId && e.MovieId == movieId);

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count -= 1;
                    _cartRepository.SaveDB();
                    TempData["notification"] = "🛒 One Ticket Removed From Your Cart!";
                }
                else if (cartItem.Count == 1)
                {
                    _cartRepository.Delete(cartItem);
                    _cartRepository.SaveDB();
                    TempData["notification"] = "🗑️ Movie Removed From Your Cart.";
                }

                TempData["MessageType"] = "Success";
            }
            else
            {
                TempData["notification"] = "Movie not found in the cart.";
                TempData["MessageType"] = "Error";
            }

            return RedirectToAction("Index");
        }

        #endregion


        #region Pay

        public IActionResult Pay()
        {
            // Get the current user ID from the UserManager
            var userId = _userManager.GetUserId(User);

            // Get the contents of the user's cart, including the movie and user data
            var cart = _cartRepository.Get(e => e.ApplicationUserId == userId, includes: [e => e.Movie, e => e.ApplicationUser]);

            // Calculate the total order from the cart
            var orderTotal = (double)cart.Sum(e => e.Movie.Price * e.Count);

            // Check if order total is less than the minimum allowed (example: 30 EGP)
            if (orderTotal < 30)
            {
                // Show a view or message to user telling them the minimum allowed
                TempData["notification"] = "The minimum payment is 25 Egyptian pounds!";
                TempData["MessageType"] = "error";

                return RedirectToAction("Index", "Cart");
            }

            // Create a new Order object and give it basic values
            var order = new MovieMart.Models.Order();
            order.ApplicationUserId = userId; // Associate the order with the user
            order.OrderDate = DateTime.Now; // Set the order date
            order.OrderTotal = (double)cart.Sum(e => e.Movie.Price * e.Count); // Calculate the total amount

            _orderRepository.Create(order);
            _orderRepository.SaveDB();

            // Set up Stripe payment session settings
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" }, // Specify the payment method (card)
                LineItems = new List<SessionLineItemOptions>(), // Payment items (movies in the cart)
                Mode = "payment", // Direct payment mode
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={order.Id}", // Success URL
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel", // Cancel URL
            };

            // Add each item from the cart to the Stripe payment options
            foreach (var item in cart)
            {
                options.LineItems.Add(
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "EGP", // Currency: Egyptian Pounds
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Title, // Movie name
                            Description = item.Movie.Description // Movie description
                        },
                        UnitAmount = (long)item.Movie.Price * 100 // Price * 100 (because Stripe uses piasters)
                    },
                    Quantity = item.Count, // Number of copies or tickets of the movie
                }
                );
            }

            // Create a Stripe payment session based on the selected options
            var service = new SessionService();
            var session = service.Create(options);

            // Associate the order with the Session ID returned by Stripe
            order.SessionId = session.Id;

            // Save the order to the database
            _orderRepository.SaveDB();

            // Create a list of items associated with the order (OrderItem)
            List<OrderItem> orderItems = [];

            foreach (var item in cart)
            {
                var orderItem = new OrderItem()
                {
                    OrderId = order.Id, // Associate the item with the current order
                    Count = item.Count, // Number of items
                    Price = (double)item.Movie.Price, // Item price
                    MovieId = item.MovieId // Movie ID                    
                };

                orderItems.Add(orderItem); // Add the item to the list
            }

            // Save the items to the database
            _orderItemRepository.CreateAll(orderItems);
            _orderItemRepository.SaveDB();

            foreach (var item in cart)
            {
                item.CartStatus = CartStatus.Paid;
                _cartRepository.Edit(item);
            }
            _cartRepository.SaveDB();

            // Redirect the user to the generated payment link
            return Redirect(session.Url);
        }

        #endregion

    }
}
