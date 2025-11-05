using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
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

                if (loggedin != null) return RedirectEmployee(loggedin); // If employee is in sesions, redirect him to his main page, bypassing login
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Forgot Password";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Email, string NewPassword, string ConfirmPassword)
        {
            ViewData["Title"] = "Forgot Password";

            try
            {
                if (string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(NewPassword) ||
                    string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    ModelState.AddModelError(string.Empty, "Please fill all fields.");
                    return View();
                }

                if (NewPassword != ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Passwords do not match.");
                    return View();
                }

                var employee = await _rep.GetByEmailAsync(Email);
                if (employee == null)
                {
                    TempData["Success"] = "If an account for that email exists, a password reset was attempted.";
                    return RedirectToAction("Login");
                }

                // Hash and persist the new password
                employee.Password = NoSQLProject.Other.Hasher.GetHashedString(NewPassword);
                await _rep.UpdateAsync(employee);

                TempData["Success"] = "Password updated successfully. Please login with your new password.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
