using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;
using System.Diagnostics;

namespace NoSQLProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _rep;

        public HomeController(IEmployeeRepository rep)
        {
            _rep = rep;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var loggedin = Authorization.GetLoggedInEmployee(HttpContext);
            if (loggedin == null)
                return RedirectToAction("Login");

            // Pass the employee as the model
            return View("Home", loggedin);
        }

        [HttpGet]
        public async Task<IActionResult> Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            try
            {
                var loggedin = Authorization.GetLoggedInEmployee(HttpContext); // Get employee from sessions

                if (loggedin != null) return RedirectEmployee(loggedin); // If employee is in sessions, redirect him to his main page, bypassing login
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
            }

            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                Employee? emp = await _rep.GetByCredentialsAsync(model.Email, Hasher.GetHashedString(model.Password)); // Get employee from db by credentials

                if (emp == null || emp.Status != Employee_Status.Active) throw new Exception("Incorrect Email or Password"); // If no employee found throw exception

                Authorization.SetLoggedInEmployee(HttpContext, emp); // Save current logged in employee in session
                HttpContext.Session.SetString("UserId", emp.Id); // Set the user ID in the session

                return RedirectEmployee(emp);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View();
            }
        }

        private RedirectToActionResult RedirectEmployee(Employee emp)
        {
            // Redirect all employees to the shared welcome page
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Authorization.RemoveLoggedInEmployee(HttpContext);

            return RedirectToAction("Login");
        }

        // GET: show the forgot-password form (now in Views/Home/Password/ForgotPassword.cshtml)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Forgot Password";
            return View("Password/ForgotPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _rep.GetByEmailAsync(email);
            if (user == null)
                return View("Password/ForgotPasswordConfirmation");

            string token = Hasher.GetHashedString(user.Id + user.Password);
            string resetLink = Url.Action("ResetPassword", "Home",
                new { userId = user.Id, token = token }, protocol: Request.Scheme);

            ViewBag.Link = resetLink;
            ViewBag.Email = email;
            return View("Password/DisplayResetLink");
        }


        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };
            return View("Password/ResetPassword", model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _rep.GetByIdAsync(model.UserId);
            if (user == null)
                return View("Password/ResetPasswordConfirmation");

            var expectedToken = Hasher.GetHashedString(user.Id + user.Password);
            if (model.Token != expectedToken)
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired token.");
                return View("Password/ResetPassword", model);
            }

            user.Password = Hasher.GetHashedString(model.NewPassword);
            await _rep.UpdateAsync(user);

            return View("Password/ResetPasswordConfirmation");
        }


        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View("Password/ResetPasswordConfirmation");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
