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
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Login"); ;
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

                if (emp == null) throw new Exception("Incorrect Email or Password"); // If no employee found throw exception

                Authorization.SetLoggedInEmployee(HttpContext, emp); // Save current logged in employee in session

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
            if (emp is ServiceDeskEmployee sde) // Depending on type of employee redirect to a different pages
            {
                return RedirectToAction("Index", "Employees");
            }
            else
            {
                return RedirectToAction("Index", "TicketsNormal");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Authorization.RemoveLoggedInEmployee(HttpContext);

            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
