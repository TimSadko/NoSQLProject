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
        //Index adjusted by Fernando for pre and post login redirections
        [HttpGet]
        public IActionResult Index()
        {
            // Check if the user is logged in (session or authentication)
            var emp = HttpContext.Session.GetObject<Employee>("LoggedInEmployee");
            if (emp == null)
            {
                // Not logged in, redirect to Login
                return RedirectToAction("Login");
            }

            // Logged in, show the Home page
            return View(emp);
        }

        [HttpGet]
        public async Task<IActionResult> Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                Employee? emp = await _rep.GetByCredentialsAsync(model.Email, Hasher.GetHashedString(model.Password)); // Get employee from db by credentials

                if (emp == null) throw new Exception("Incorrect Email or Password"); // If no employee found throw exception

                HttpContext.Session.SetObject("LoggedInEmployee", emp); // Save current logged in employee in session

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Ends the session
            return RedirectToAction("Login", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
