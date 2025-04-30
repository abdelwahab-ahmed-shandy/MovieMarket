using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using MovieMart.Models.ViewModels;
using MovieMart.Repositories.IRepositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MovieMart.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApplicationUserRepository _applicationUserRepository;

        public SettingsController(UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager,
                                      SignInManager<ApplicationUser> signInManager,
                                         IApplicationUserRepository applicationUserRepository)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
            this._applicationUserRepository = applicationUserRepository;
        }


        #region Profile

        // Get profile data for the currently logged-in user
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Get the current user's data through the UserManager
            var user = await _userManager.GetUserAsync(User);

            // If the user is not found, they are redirected to the "Not Found" page
            if (user == null)
            {
                return RedirectToAction("NotFound", "Account", new { area = "Identity" });
            }

            // Set the page title in ViewData to display in the UI
            ViewData["Title"] = "Profile";

            // Create a ViewModel object containing user data to display in the UI
            var profile = new ProfileVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address = user.Address,
                UserName = user.UserName
            };

            // Display the profile page and send data to it
            return View(profile);
        }

        #endregion


        #region Manage Profile

        // Display the profile edit page
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Account", new { area = "Identity" });
            }

            var profile = new ManageProfileVM
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Address = user.Address
            };

            return View(profile);
        }

        // Process profile modifications after form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageProfileVM manageProfileVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("NotFound", "Account", new { area = "Identity" });
            }

            // Verify the current password
            var passwordValid = await _userManager.CheckPasswordAsync(user, manageProfileVM.CurrentPassword);
            if (!passwordValid)
            {
                ModelState.AddModelError("CurrentPassword", "The current password is incorrect");
                return View(manageProfileVM);
            }

            // Update user data
            user.FirstName = manageProfileVM.FirstName;
            user.LastName = manageProfileVM.LastName;
            user.UserName = manageProfileVM.UserName;
            user.Address = manageProfileVM.Address; // Check that the statement is valid

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(manageProfileVM);
            }

            // Use the same keys in the view

            TempData["notifiction"] = "Profile updated successfully";

            TempData["MessageType"] = "Success";

            return RedirectToAction("Profile", "Settings", new { area = "Identity" });
        }
        #endregion


        #region Change Password

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);

                var result = await _userManager.ChangePasswordAsync(user, changePasswordVM.OldPassword, changePasswordVM.NewPassword);

                if (result.Succeeded)
                {
                    TempData["notifiction"] = "Password changed successfully";
                    TempData["MessageType"] = "Success";
                    return RedirectToAction("Profile", "Settings");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed
            TempData["notifiction"] = "Failed to change password";
            TempData["MessageType"] = "Notice";
            return RedirectToAction("Profile", "Settings");
        }

        #endregion


        #region Delete Customer (No one uses the delete button except the account owner.)

        // Displays the account deletion confirmation page when requested via HTTP GET
        [HttpGet]
        public IActionResult DeleteAccount()
        {
            return View();
        }

        // Processes the account deletion request when sent via HTTP POST
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against CSRF (Cross-Site Request Forgery) attacks
        public async Task<IActionResult> ConfirmDeleteAccount()
        {
            // Gets the current user via UserManager
            var user = await _userManager.GetUserAsync(User);

            // If the user is not found, an error message is displayed and redirects to the home page
            if (user == null)
            {
                TempData["notifiction"] = "User not found.";
                TempData["MessageType"] = "Error";
                return RedirectToAction("Index", "Home");
            }

            // Checks if the user has the "Admin" or "SuperAdmin" role
            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                // Prevents account deletion if the user is an administrator (Admin or SuperAdmin)
                TempData["notifiction"] = "Admins and SuperAdmins cannot delete their own accounts.";
                TempData["MessageType"] = "Error";
                return RedirectToAction("Profile");
            }

            // Updates the Security Stamp to ensure session security before deleting the account
            await _userManager.UpdateSecurityStampAsync(user);

            // Deletes the account via the UserManager
            var result = await _userManager.DeleteAsync(user);

            // If the deletion fails, an error message is displayed and the user is redirected to the profile page.
            if (!result.Succeeded)
            {

                // Set the success message in TempData
                TempData["notifiction"] = "An error occurred while deleting the account.";
                TempData["MessageType"] = "error";

                return RedirectToAction("Profile");
            }

            // Signs the user out after deleting their account.

            await _signInManager.SignOutAsync();

            // Displays a success message and redirects the user to the "Customer" section home page.
            TempData["notifiction"] = "Your account has been successfully deleted.";
            TempData["MessageType"] = "Warning";

            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        #endregion


    }
}
