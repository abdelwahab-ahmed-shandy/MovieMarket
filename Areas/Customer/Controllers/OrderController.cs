using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Linq.Expressions;
using SessionService = Stripe.Checkout.SessionService;

namespace MovieMart.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICartRepository _cartRepository;
        public OrderController(UserManager<ApplicationUser> userManager, IOrderRepository orderRepository,
                                                                           IOrderItemRepository orderItemRepository,
                                                                           ICartRepository cartRepository)
        {
            this._userManager = userManager;
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
            this._cartRepository = cartRepository;
        }

        #region Show My Orders

        public IActionResult MyOrders()
        {
            var GetUserId = _userManager.GetUserId(User);

            var GetOrders = _orderRepository.Get(o => o.ApplicationUserId == GetUserId,
                                            includes: [o => o.ApplicationUser]).OrderByDescending(o => o.OrderDate);


            return View(GetOrders);
        }

        #endregion

        #region Refund Order
        public IActionResult Refund(int orderId)
        {
            var order = _orderRepository.GetOne(filter: e => e.Id == orderId);

            if (order is not null)
            {
                RefundCreateOptions options = new()
                {
                    Amount = (long)order.OrderTotal,
                    PaymentIntent = order.PaymentStripeId,
                    Reason = RefundReasons.Fraudulent
                };

                var service = new RefundService();
                var session = service.Create(options);

                order.Status = OrderStatus.Canceled;
                _orderRepository.SaveDB();

                return RedirectToAction("Details");
            }

            return RedirectToAction("NotFoundPage", "Home", new { area = "Customer" });
        }

        #endregion
    }
}
