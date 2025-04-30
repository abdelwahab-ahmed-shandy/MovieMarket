using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace MovieMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // todo:::here
        //public IActionResult AllOrders()
        //{
        //    var orders = _orderRepository.Get(includes: [o => o.ApplicationUser, o => o.  ])
        //        .Include(o => o.ApplicationUser)
        //        .Include(o => o.OrderItems)



        //    return View(orders.ToList());
        //}


    }
}
