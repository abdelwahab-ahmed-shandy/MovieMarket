using Microsoft.AspNetCore.Mvc;

namespace MovieMart.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class NewsletterController : Controller
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly ILogger<NewsletterController> _logger;

        public NewsletterController(ISubscriberRepository subscriberRepository, ILogger<NewsletterController> logger)
        {
            _subscriberRepository = subscriberRepository;
            _logger = logger;
        }

        #region Private Methods

        private IActionResult RedirectToReferer()
        {
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(email))
                {
                    TempData["notification"] = "Please enter a valid email address.";
                    TempData["MessageType"] = "error";
                    return RedirectToReferer();
                }

                email = email.Trim().ToLower();

                var existingSubscriber = await _subscriberRepository.GetFirstAsync(
                    s => s.Email == email,
                    cancellationToken: HttpContext.RequestAborted
                );

                if (existingSubscriber != null)
                {
                    TempData["notification"] = "You are already subscribed!";
                    TempData["MessageType"] = "warning";
                    return RedirectToReferer();
                }

                await _subscriberRepository.CreateAsync(new Subscriber { Email = email }, HttpContext.RequestAborted);
                await _subscriberRepository.SaveChangesAsync(HttpContext.RequestAborted);

                TempData["notification"] = "Thank you for subscribing!";
                TempData["MessageType"] = "success";

                _logger.LogInformation("New subscriber: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing email: {Email}", email);
                TempData["notification"] = "An error occurred. Please try again.";
                TempData["MessageType"] = "error";
            }

            return RedirectToReferer();
        }



    }
}

