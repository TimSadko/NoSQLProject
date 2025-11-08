using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Services;

public class EmployeesController : Controller
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string sortField = "Status", int sortOrder = 1, string status = "All")
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            List<Employee> employees;

            if (string.IsNullOrEmpty(status) || status == "All")
            {     
                employees = await _employeeService.GetAllEmployeesSortedAsync(sortField, sortOrder); // Default: show all employees sorted
            }
            else
            {           
                employees = await _employeeService.GetEmployeesByStatusAsync(status); // Filter by status if provided
            }

            ViewBag.SortField = sortField;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Status = status;

            return View(employees);
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return View();
        }
    }

    [HttpGet]
    public IActionResult Add()
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Add(Employee employee, string Role)
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            if (!ModelState.IsValid) throw new Exception("The model is invalid");

            Employee toAdd;

            if (Role == "ServiceDeskEmployee")
            {
                toAdd = new ServiceDeskEmployee
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Password = employee.Password,
                    Status = employee.Status
                };
            }
            else
            {
                toAdd = employee;
            }

            await _employeeService.AddEmployeeAsync(toAdd);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return View(employee);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) throw new Exception("Employee not found");
            return View(employee);
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Employee employee, string Role)
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            if (!ModelState.IsValid) throw new Exception("The model is invalid");

            Employee toUpdate;

            if (Role == "ServiceDeskEmployee")
            {
                var existing = await _employeeService.GetEmployeeByIdAsync(employee.Id);
                var managed = (existing as ServiceDeskEmployee)?.ManagedEmployees ?? new List<string>();

                toUpdate = new ServiceDeskEmployee
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Password = employee.Password,
                    Status = employee.Status,
                    ManagedEmployees = managed
                };
            }
            else
            {
                toUpdate = employee;
            }

            await _employeeService.UpdateEmployeeAsync(toUpdate);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return View(employee);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) throw new Exception("Employee not found");
            return View(employee);
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id)
                throw new Exception("You cannot delete your own account while logged in.");

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee != null)
                await _employeeService.DeleteEmployeeAsync(employee);

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ManagedEmployees(string id)
    {
        if (!Authenticate()) return RedirectToAction("Login", "Home");

        try
        {
            var managedEmployees = await _employeeService.GetEmployeesManagedByAsync(id);
            return View(managedEmployees ?? new List<Employee>());
        }
        catch (Exception ex)
        {
            TempData["Exception"] = ex.Message;
            return View(new List<Employee>());
        }
    }

    public bool Authenticate()
    {
        Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);
        return emp is ServiceDeskEmployee;
    }
}
