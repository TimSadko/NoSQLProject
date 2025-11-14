using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;
using NoSQLProject.Services;
using System.Diagnostics;

namespace NoSQLProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _rep;
        private readonly PasswordResetService _passwordResetService;

        public HomeController(IEmployeeRepository rep, PasswordResetService passwordResetService)
        {
            _rep = rep;
            _passwordResetService = passwordResetService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var loggedin = Authorization.GetLoggedInEmployee(HttpContext);
            if (loggedin == null)
                return RedirectToAction("Login");

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
                var loggedin = Authorization.GetLoggedInEmployee(HttpContext); 

                if (loggedin != null) return RedirectEmployee(loggedin); 
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
                Employee? emp = await _rep.GetByCredentialsAsync(model.Email, Hasher.GetHashedString(model.Password)); 

                if (emp == null || emp.Status != Employee_Status.Active) throw new Exception("Incorrect Email or Password"); 

                Authorization.SetLoggedInEmployee(HttpContext, emp); 
                HttpContext.Session.SetString("UserId", emp.Id); 

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
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Authorization.RemoveLoggedInEmployee(HttpContext);

            return RedirectToAction("Login");
        }

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

            string token = _passwordResetService.GenerateTokenForUser(user);
            string resetLink = Url.Action("ResetPassword", "Home", 
                new { userId = user.Id, token }, protocol: Request.Scheme);

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
            var success = await _passwordResetService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired token.");
                return View("Password/ResetPassword", model);
            }

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
