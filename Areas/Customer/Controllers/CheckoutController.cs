using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.BillingPortal;
using Stripe.Checkout;
using SessionService = Stripe.Checkout.SessionService;

namespace MovieMart.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly MovieMarketDbContext _movieMarketDbContext;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;

        public CheckoutController(IOrderRepository orderRepository, MovieMarketDbContext movieMarketDbContext,
                                                                        IOrderItemRepository orderItemRepository,
                                                                            UserManager<ApplicationUser> userManager,
                                                                              ICartRepository cartRepository)
        {
            _orderRepository = orderRepository;
            _movieMarketDbContext = movieMarketDbContext;
            _orderItemRepository = orderItemRepository;
            _userManager = userManager;
            _cartRepository = cartRepository;

        }

        public IActionResult Success(int orderId)
        {
            using var transaction = _movieMarketDbContext.Database.BeginTransaction();
            var appUserId = _userManager.GetUserId(User);

            try
            {
                var order = _orderRepository.GetOne(e => e.Id == orderId);

                if (order != null && order.PaymentStatus == false)
                {
                    order.Status = OrderStatus.InProgress;
                    order.PaymentStatus = true;

                    var service = new SessionService();
                    var session = service.Get(order.SessionId);

                    order.PaymentStripeId = session.PaymentIntentId;

                    _orderRepository.SaveDB();

                    var carts = _cartRepository.Get(e => e.ApplicationUserId == appUserId, [e => e.Movie]);

                    List<OrderItem> orderItems = new();
                    foreach (var item in carts)
                    {
                        orderItems.Add(new()
                        {
                            Count = item.Count,
                            OrderId = orderId,
                            Price = item.Movie.Price,
                            MovieId = item.MovieId
                        });
                    }

                    _orderItemRepository.CreateAll(orderItems);
                    _orderItemRepository.SaveDB();

                    _cartRepository.DeleteAll(carts.ToList());
                    _cartRepository.SaveDB();

                    transaction.Commit();

                    return View();
                }

                return NotFound();
            }
            catch
            {
                transaction.Rollback();

                return NotFound();
            }
        }

        public IActionResult Cancel()
        {
            return View();
        }
    }
}
