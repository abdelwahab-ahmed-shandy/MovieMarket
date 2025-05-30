using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieMart.Models;

namespace MovieMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsletterController : Controller
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IEmailSender _emailSender;
        private readonly ISentEmailRepository _sentEmailRepository;
        public NewsletterController(ISubscriberRepository subscriberRepository, IEmailSender emailSender
                                        , ISentEmailRepository sentEmailRepository)
        {
            _subscriberRepository = subscriberRepository;
            _emailSender = emailSender;
            _sentEmailRepository = sentEmailRepository;
        }


        #region View All Letters
        public async Task<IActionResult> Index(string? query, int page = 1)
        {
            var subscribers = await _subscriberRepository
                .GetQuery()
                .OrderByDescending(s => s.SubscribedAt)
                .ToListAsync();

            if (query != null)
            {
                subscribers = subscribers.Where(e => (e.Email ?? "").Contains(query)
                || (e.SubscribedAt.ToString("yyyy-MM-dd") ?? "").Contains(query)
                ).ToList();
            }

            int pageSize = 5;
            int totalmovie = subscribers.Count();
            int totalPages = (int)Math.Ceiling((double)totalmovie / pageSize);

            if (page > totalPages && totalPages > 0)
                return NotFound();

            subscribers = subscribers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.totalPages = totalPages;
            ViewBag.currentPage = page;

            return View(subscribers);
        }

        #endregion



        #region Send Email

        [HttpGet]
        public IActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(string subject, string body)
        {
            var subscribers = await _subscriberRepository.GetQuery().ToListAsync();

            foreach (var subscriber in subscribers)
            {
                // Replace this with your actual email sending service
                await _emailSender.SendEmailAsync(subscriber.Email, subject, body);
            }

            var sentEmail = new SentEmail
            {
                Subject = subject,
                Body = body,
                SentAt = DateTime.Now
            };

            _sentEmailRepository.Create(sentEmail);
            await _sentEmailRepository.SaveChangesAsync();


            TempData["notification"] = "Email sent successfully to all subscribers.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(SendEmail));
        }

        #endregion



        #region Email History

        public async Task<IActionResult> EmailHistory()
        {
            var emails = await _sentEmailRepository.GetQuery()
                .OrderByDescending(e => e.SentAt)
                .ToListAsync();

            return View(emails);
        }


        #endregion


    }
}
