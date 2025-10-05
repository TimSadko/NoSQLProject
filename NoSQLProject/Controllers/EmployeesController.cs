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
        public async Task<IActionResult> Add(Employee employee, string Role)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            if (!ModelState.IsValid)
                return View(employee);

            try
            {
                Employee newEmployee;
                if (Role == "ServiceDeskEmployee")
                {
                    newEmployee = new ServiceDeskEmployee
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Email = employee.Email,
                        Password = employee.Password,
                        Status = employee.Status,
                        ManagedEmployeesId = new List<string>() // or null/empty as needed
                    };
                }
                else
                {
                    newEmployee = new Employee
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Email = employee.Email,
                        Password = employee.Password,
                        Status = employee.Status
                    };
                }

                await _rep.Add(newEmployee);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
                return View(employee);
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

            // Get the currently logged-in employee
            var loggedIn = Authorization.GetLoggedInEmployee(this.HttpContext);

            // Prevent self-deletion
            if (loggedIn != null && loggedIn.Id == employee.Id)
            {
                ModelState.AddModelError("", "You cannot delete your own account while logged in.");
                return View(employee);
            }

            await _rep.Delete(employee);
            return RedirectToAction("Index");
        }
        // Testing Manages 
        [HttpGet]
        public async Task<IActionResult> ManagedEmployees(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            var employee = await _rep.GetByIdAsync(id);
            if (employee is not ServiceDeskEmployee sde)
                return NotFound();

            // Fetch full Employee objects for each managed ID
            var managedEmployees = new List<Employee>();
            if (sde.ManagedEmployeesId != null)
            {
                foreach (var empId in sde.ManagedEmployeesId)
                {
                    var managedEmp = await _rep.GetByIdAsync(empId);
                    if (managedEmp != null)
                        managedEmployees.Add(managedEmp);
                }
            }

            // Pass both the ServiceDeskEmployee and the managed employees to the view
            ViewBag.ServiceDeskEmployee = sde;
            return View(managedEmployees);
        }

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);
            if(emp == null || emp is not ServiceDeskEmployee) return false;
            return true;
        }
    }
}
