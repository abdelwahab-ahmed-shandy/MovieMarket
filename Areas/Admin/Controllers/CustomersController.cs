﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieMart.Models.ViewModels;
using MovieMart.Repositories;
using MovieMart.Repositories.IRepositories;
using MovieMart.Models;
using System.Linq.Expressions;

namespace MovieMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CustomersController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomersController> _logger;
        public CustomersController(IApplicationUserRepository applicationUserRepository,
                                    UserManager<ApplicationUser> userManager,
                                    ILogger<CustomersController> logger)
        {
            this._applicationUserRepository = applicationUserRepository;
            this._userManager = userManager;
            this._logger = logger;
        }

        #region View All Customers
        // Define an Action to retrieve all customers
        [HttpGet]
        public async Task<IActionResult> AllCustomers(string? query, int page = 1)
        {
            // Retrieve all users from the database without tracking entities
            IEnumerable<ApplicationUser> allUsers = _applicationUserRepository.Get().AsNoTracking().ToList();

            // Create a list to store only customers
            var allCustomer = new List<ApplicationUser>();

            // Filter users so that only those with the "Customer" role and not those with the "Admin" or "SuperAdmin" role are retained
            foreach (var user in allUsers)
            {
                var role = await _userManager.GetRolesAsync(user); // Get user roles

                if (!role.Contains("Admin") && !role.Contains("SuberAdmin") && role.Contains("Customer"))
                {
                    allCustomer.Add(user); // Add the user to the customer list
                }
            }

            // Check if there is a search query
            if (query != null)
            {
                allCustomer = allCustomer.Where(e => (e.UserName ?? "").Contains(query) // Search for the username
                || (e.Email ?? "").Contains(query) // Search for the email
                || (e.FirstName ?? "").Contains(query) // Search for the first name
                || (e.LastName ?? "").Contains(query) // Search for the last name
                || (e.Address ?? "").Contains(query) // Search for the address
                || (e.Id ?? "").Contains(query)) // Search for the ID number
                .ToList();
            }

            // Calculate the number of pages required, so that there are 5 customers per page
            int pageSize = 5;
            int totalCustomers = allCustomer.Count();
            int totalPages = (int)Math.Ceiling((double)totalCustomers / pageSize);

            // Check if the requested page does not exist
            if (page > totalPages && totalPages > 0)
                return NotFound();

            // Split the customers and display only 5 per page
            allCustomer = allCustomer.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Pass the number of pages to the View
            ViewBag.totalPages = totalPages;
            ViewBag.currentPage = page;

            // Return the list of customers to the View
            return View(allCustomer.ToList());
        }
        #endregion


        #region Create Customer

        // Display the new customer creation form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // Process a new customer creation request when the form is submitted
        [HttpPost]
        [ValidateAntiForgeryToken] // Protection against CSRF attacks
        public async Task<IActionResult> Create(ApplicationUser applicationUser)
        {
            // Validate the entered data before saving
            if (ModelState.IsValid)
            {
                // Add the new customer to the database
                _applicationUserRepository.Create(applicationUser);

                // Assign the "Customer" role to the new user
                await _userManager.AddToRoleAsync(applicationUser, "Customer");

                //Save changes to the database
                _applicationUserRepository.SaveDB();

                // Store a success message in TempData to display after redirection
                TempData["notification"] = "The Customer was created successfully!";
                TempData["MessageType"] = "success";

                // Redirect to the All Customers view page
                return RedirectToAction(nameof(AllCustomers));
            }

            // If there is an input error, re-render the form with the same data
            return View(applicationUser);
        }

        #endregion


        #region Block Customer Account :

        // This action is responsible for blocking a customer's account based on their ID
        public async Task<IActionResult> Block(string Id)
        {
            // Attempt to retrieve the user from the database using the provided ID
            var userDB = await _userManager.FindByIdAsync(Id);

            // Check if the user was found
            if (userDB != null)
            {
                // Set the 'IsBlocked' flag to true to block the user
                userDB.IsBlocked = true;

                // Update the user data in the database
                var result = await _userManager.UpdateAsync(userDB);

                // Check if the update was successful
                if (result.Succeeded)
                {
                    // Display a success message to the admin
                    TempData["Message"] = "The customer's account has been successfully banned.";
                    TempData["MessageType"] = "Warning";

                    // Optional: Log the blocking action for auditing and security tracking
                    _logger.LogInformation($"User {userDB.Email} has been blocked.");
                }
                else
                {
                    // If update failed, show an error message
                    TempData["Message"] = "An error occurred while blocking the account.";
                    TempData["MessageType"] = "error";
                }

                // Redirect back to the list of all customers
                return RedirectToAction("AllCustomers");
            }

            // If the user was not found, display an appropriate error message
            TempData["Message"] = "Client not found?!";
            TempData["MessageType"] = "error";

            // Redirect to the customers list even if the user wasn't found
            return RedirectToAction("AllCustomers");
        }

        #endregion


        #region Un Block Customer Account :

        // This action is responsible for unblocking a customer's account based on their ID
        public async Task<IActionResult> UnBlock(string Id)
        {
            // Attempt to retrieve the user from the database using the provided ID
            var userDB = await _userManager.FindByIdAsync(Id);

            // Check if the user was found
            if (userDB != null)
            {
                // Set the 'IsBlocked' flag to false to unblock the user
                userDB.IsBlocked = false;

                // Update the user data in the database 
                var result = await _userManager.UpdateAsync(user: userDB);

                // Check if the update was successful
                if (result.Succeeded)
                {
                    // Display a success message to the admin
                    TempData["Message"] = "The customer's account has been successfully unblocked.";
                    TempData["MessageType"] = "Success";

                    // Optional: Log the unblocking action for auditing and security tracking
                    _logger.LogInformation($"User {userDB.Email} has been unblocked.");
                }
                else
                {
                    // If update failed, show an error message
                    TempData["Message"] = "An error occurred while unblocking the account.";
                    TempData["MessageType"] = "error";
                }
                // Redirect back to the list of all customers
                return RedirectToAction("AllCustomers");
            }
            // If the user was not found, display an appropriate error message
            TempData["Message"] = "Client not found?!";
            TempData["MessageType"] = "error";

            // Redirect to the customers list even if the user wasn't found
            return RedirectToAction("AllCustomers");
        }

        #endregion


        #region Edit Customer (I stopped trying this action because I found it unreasonable for someone to modify someone else's file )

        //[HttpGet]
        //public IActionResult Edit(string Id)
        //{
        //    var user = _applicationUserRepository.GetOne(u => u.Id == Id);
        //    if (user != null)
        //    {
        //        var userEdit = new ManageProfileVM
        //        {
        //            FirstName = user.FirstName,
        //            LastName = user.LastName,
        //            Address = user.Address,
        //            UserName = user.UserName,
        //            Email = user.Email

        //        };
        //        return View(user);
        //    }
        //    return RedirectToAction("NotFound", "Account", new { area = "Identity" });
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(ManageProfileVM model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = _applicationUserRepository.GetOne(u => u.Id == model.Id as string);
        //    if (user == null)
        //    {
        //        return RedirectToAction("NotFound", "Account", new { area = "Identity" });
        //    }

        //    user.FirstName = model.FirstName;
        //    user.LastName = model.LastName;
        //    user.UserName = model.UserName;
        //    user.Email = model.Email;
        //    user.Address = model.Address;

        //    _applicationUserRepository.Edit(user);
        //    _applicationUserRepository.SaveDB();

        //    TempData["notification"] = "Edit Customer Successfully!";
        //    TempData["MessageType"] = "Success";

        //    return RedirectToAction(nameof(Index));
        //}
        #endregion
    }
}
