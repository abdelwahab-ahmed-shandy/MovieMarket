using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using MovieMart.Models.ViewModels;
using MovieMart.Services.IServices;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MovieMart.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._emailSender = emailSender;
        }

        #region Register

        //  Display the registration page when requested using HTTP GET
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            //  Check if roles do not exist, and create them if necessary
            if (_roleManager.Roles.IsNullOrEmpty())
            {
                await _roleManager.CreateAsync(role: new IdentityRole("SuperAdmin")); // Create the "SuperAdmin" role
                await _roleManager.CreateAsync(role: new IdentityRole("Admin")); // Create the "Admin" role
                await _roleManager.CreateAsync(role: new IdentityRole("Customer")); // Create the "Customer" role
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
            return View(); //  Display the registration page for the user
        }

        //  Process registration data when sent using HTTP POST
        [HttpPost]
        [ValidateAntiForgeryToken] //  Protection against CSRF attacks
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            //  Check if the form data is valid
            if (ModelState.IsValid)
            {
                //  Create a new user object based on the data entered in the form
                ApplicationUser applicationUser = new ApplicationUser
                {
                    FirstName = registerVM.FirstName,
                    LastName = registerVM.LastName,
                    Address = registerVM.Address,
                    UserName = registerVM.UserName,
                    Email = registerVM.Email
                };

                //  Create a user account in the system
                var newUser = await _userManager.CreateAsync(applicationUser, registerVM.Password);

                //  Check if the user was created successfully
                if (newUser.Succeeded)
                {
                    //  Generate Email Confirmation Token
                    var userId = applicationUser.Id;
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                    var returnUrl = Url.Content("~/"); // 🔙 Return URL after confirmation

                    //  Generate Email Confirmation Link
                    var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme
                    );

                    //  Send an email to the user with a confirmation link
                    await _emailSender.SendEmailAsync(registerVM.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //  Add a notification to the user informing them of the need to confirm their email
                    TempData["notification"] = "Your account has been created! Please check your email to confirm the account before logging in";
                    TempData["MessageType"] = "Success";

                    //  Automatically add the user to the "Customer" role
                    await _userManager.AddToRoleAsync(applicationUser, "Customer");

                    //  If the email address is already confirmed, login is immediate.
                    if (applicationUser.EmailConfirmed)
                    {
                        await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }

                    //  Direct the user to the login page if they haven't confirmed their email address yet.
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }
                else
                {
                    //  If registration fails, errors are displayed to the user.
                    foreach (var error in newUser.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            //  Redisplay the registration page if there are input errors.
            return View(registerVM);
        }

        #endregion


        #region Login

        // Display the login page when requested via HTTP GET
        [HttpGet]

        public async Task<IActionResult> Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            // Display the login interface

            return View();
        }

        // Process login data when sent via HTTP POST
        [HttpPost]
        [ValidateAntiForgeryToken] // Protection against CSRF attacks
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            // Validate the entered data
            if (ModelState.IsValid)
            {
                // Find the user using email
                var user = await _userManager.FindByEmailAsync(loginVM.Email);

                if (user != null)
                {
                    //
                    if (user.IsBlocked)
                    {
                        ModelState.AddModelError("", "Your account is blocked. Please contact support.");
                        return View(loginVM);
                    }

                    // Validate the entered password
                    var checkPassByUser = await _userManager.CheckPasswordAsync(user, loginVM.Password);

                    if (checkPassByUser)
                    {
                        // User login successfully
                        await _signInManager.SignInAsync(user, loginVM.RememberMe);

                        //
                        if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                        {

                            // Redirect the user to the home page in the "Admin" area
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        else
                        {
                            // Redirect the user to the home page in the "Customer" area
                            return RedirectToAction("Index", "Home", new { area = "Customer" });
                        }

                    }
                    else
                    {
                        // Add an error message if the password does not match
                        ModelState.AddModelError("Password", "Invalid password");
                        ModelState.AddModelError("Email", "Email not found");
                    }
                }
                else
                {
                    // Add an error message if the email address is not found
                    ModelState.AddModelError("Password", "Invalid password");
                    ModelState.AddModelError("Email", "Email not found");
                }
            }

            // Re-display the login page with the entered data in case of errors
            return View(loginVM);
        }

        #endregion


        #region Forget Password

        // Display the "Forgot Password" page (GET)
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        // Receive form from "Forgot Password" page (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM model)
        {
            // Check the validity of the entered data
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user using email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Not disclosing that the mail is not registered for security reasons
                TempData["notification"] = "If your email is registered, you'll receive a password reset link";
                TempData["MessageType"] = "info";
                return RedirectToAction("ForgetPasswordConfirmation");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create a password reset link
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { email = model.Email, token = token }, protocol: HttpContext.Request.Scheme);

            // Send an email containing a password reset link
            await _emailSender.SendEmailAsync(
                model.Email,
                "Reset your MovieMart password",
                $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

            // Notify the user that the link has been sent
            TempData["notification"] = "Password reset link has been sent to your email";
            TempData["MessageType"] = "success";
            return RedirectToAction("ForgetPasswordConfirmation");
        }


        // Display the confirmation page for sending the reset link (GET)
        [HttpGet]
        public IActionResult ForgetPasswordConfirmation()
        {
            return View();
        }

        // Display the password reset form (GET) based on the sent link
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            // Check if the code and email are present
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }

            // Set up the form with the postal and code values
            var forgetPassword = new ForgetPasswordVM
            {
                Email = email,
                ResetToken = token
            };

            return View(forgetPassword);
        }

        // Receive password reset form (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ForgetPasswordVM model)
        {
            // Data validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user using email
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Do not detect that the user does not exist
                TempData["notification"] = "Password has been reset successfully";
                TempData["MessageType"] = "success";
                return RedirectToAction("ResetPasswordConfirmation");
            }

            // Perform password reset operation using token
            var result = await _userManager.ResetPasswordAsync(user, model.ResetToken, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["notification"] = "Password has been reset successfully";
                TempData["MessageType"] = "success";
                return RedirectToAction("ResetPasswordConfirmation");
            }

            // If there are errors, display them to the user.
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Display the password reset success confirmation page (GET)
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion


        #region External Login

        // This action is called when the user clicks the "Sign in with Google or another external provider" button
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Specifies the URL to which the user will return after completing the login process with the external provider (e.g., Google)
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });

            // Configures the external login properties, such as the provider name (Google/Facebook) and the return URL
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // The user is directed to the external provider's login page (e.g., Google)
            return Challenge(properties, provider);
        }

        // This action is called automatically after the user returns from an external login provider (e.g., Google)
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            // If returnUrl is not specified, it is set to the home page.
            returnUrl ??= Url.Content("~/");

            // Checks if there is an error sent by the service provider (e.g., user authorization denied or connection error)
            if (!string.IsNullOrEmpty(remoteError))
            {

                TempData["Message"] = $"Service provider error: {remoteError}";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Login)); // Redirect the user to the login page
            }

            // Retrieve external login information (such as the user ID from Google)
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {

                TempData["Message"] = "Failed to retrieve login information from Google.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            // Attempt to log in using the external provider information if a Google account is already linked
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, // Provider name (such as Google)
            info.ProviderKey, // The user's unique identifier from the provider
            isPersistent: false, // Remember me is not enabled
            bypassTwoFactor: true // Bypass two-factor authentication if present
            );

            // If the login process was successful
            if (signInResult.Succeeded)
            {
                TempData["SuccessMessage"] =
                TempData["notification"] = "Successfully logged in with Google.";
                TempData["MessageType"] = "success";
                return LocalRedirect(returnUrl); // Redirect to the page the user requested
            }

            // Extract the email from the Google account (used to create or link the local account)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // If the email was not retrieved (may be hidden by Google's privacy settings)
            if (email == null)
            {
                TempData["Message"] = "Email not retrieved from Google account.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            // Find a local user with the same email
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // If the user exists, an attempt is made to link their Google account.
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["notification"] = "Google account linked and login successful.";
                    TempData["MessageType"] = "success";
                    return LocalRedirect(returnUrl);
                }

                // If there are errors when linking the account, they are displayed in the interface.
                foreach (var error in addLoginResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                TempData["Message"] = "An error occurred while linking the Google account.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Login));
            }

            // If the user has not previously registered, a new account is automatically created for them.
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true, // Automatically confirm the email address
                FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                Address = info.Principal.FindFirstValue(ClaimTypes.StreetAddress),
                IsBlocked = false // Set the user as not blocked

            };

            // Create the user in the database
            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                // Attempt to associate a Google account with the new user
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["notification"] = "Account created and successful sign-in via Google.";
                    TempData["MessageType"] = "success";
                    return LocalRedirect(returnUrl);
                }

                // If the account was created but linking with Google failed
                TempData["Message"] = "Account created, but Google account linking failed.";
                TempData["MessageType"] = "error";
            }
            else
            {
                // If account creation failed, all errors are displayed
                TempData["Message"] = "Account creation failed.";
                TempData["MessageType"] = "error";
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            // In all previous failed cases, the user is returned to the login page
            return RedirectToAction(nameof(Login));
        }

        #endregion


        #region LogOut

        // Log the user out
        public async Task<IActionResult> Logout()
        {
            // Perform the logout operation
            await _signInManager.SignOutAsync();

            // Redirect the user to the login page
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        #endregion


        #region AccDenied, NotFound, and IntSerErr

        // Display the "Access Denied" page when attempting to access an unauthorized resource
        public IActionResult AccessDenied() => View();
        // Display the "Not Found" page when attempting to access a non-existent page
        public IActionResult NotFound() => View();
        // Display the "Internal Server Error" page when an internal server error occurs
        public IActionResult InternalServerError() => View();

        #endregion


        /*
        Services:
        */
        #region ConfirmEmail (Beginning of the email confirmation section )

        // An asynchronous (Async) function used to confirm a user's email
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            // Checks that the entered values ​​are correct (userId and code must not be empty)
            if (userId == null || code == null)
            {
                return NotFound("Invalid email confirmation request."); // If the data is invalid, a 404 error is returned with a message explaining the problem
            }

            // Find the user in the database using their userId
            var user = await _userManager.FindByIdAsync(userId);

            // Checks if the user does not exist
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'."); // Return a 404 error if the user is not found
            }

            // Perform the email confirmation process using code
            var result = await _userManager.ConfirmEmailAsync(user, code);

            // Check if the confirmation process was successful
            if (result.Succeeded)
            {
                // Automatically log the user in after the email confirmation process is successful
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Store a notification in TempData to display to the user in the interface
                TempData["notification"] = "Your email has been successfully confirmed! You have been automatically logged in.";
                TempData["MessageType"] = "Success"; // Specify the message type as success

                // Redirect the user to the profile page within the "Identity" area
                return RedirectToAction("Profile", "Settings", new { area = "Identity" });
            }

            // If the confirmation process fails, an error page is displayed.
            return View("Error");
        }
        #endregion // End of the email confirmation section


    }
}
