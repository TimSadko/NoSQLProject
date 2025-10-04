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
        //CRUD added by Fernando:
        [HttpGet]
        public IActionResult Add()
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Employee employee)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");
            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                await _rep.Add(employee);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(employee);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            var employee = await _rep.GetByIdAsync(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                await _rep.Update(employee);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(employee);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");
            var employee = await _rep.GetByIdAsync(id);
            if (employee == null)
                return NotFound();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Employee employee)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");
            await _rep.Delete(employee);
            return RedirectToAction("Index");
        }

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);
            if(emp == null || emp is not ServiceDeskEmployee) return false;
            return true;
        }
    }
}
