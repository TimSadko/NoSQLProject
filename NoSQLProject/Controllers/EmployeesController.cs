using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;

namespace NoSQLProject.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _rep;

        public EmployeesController(IEmployeeRepository rep)
        {
            _rep = rep;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if(!Authenticate()) return RedirectToAction("Login", "Home"); // If user is not logged in or not the right type, redirect to login page

            try
            {
                var list = await _rep.GetAllAsync();

                return View(list);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View();
            }
        }

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);

            if(emp == null || emp is not ServiceDeskEmployee) return false;
             
            return true;
        }
    }
}
