using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Services;

public class EmployeesController : Controller
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return View(employees);
    }

    public IActionResult Add() => View();
    //prepare FOR CONTROL Z
    [HttpPost]
    public async Task<IActionResult> Add(Employee employee, string Role)
    {
        if (ModelState.IsValid)
        {
            Employee toAdd;
            if (Role == "ServiceDeskEmployee")
            {
                toAdd = new ServiceDeskEmployee
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Password = employee.Password,
                    Status = employee.Status,
                    // Optionally initialize ManagedEmployees if needed
                };
            }
            else
            {
                toAdd = employee;
            }

            await _employeeService.AddEmployeeAsync(toAdd);
            return RedirectToAction("Index");
        }
        return View(employee);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Employee employee, string Role)
    {
        if (ModelState.IsValid)
        {
            Employee toUpdate;
            if (Role == "ServiceDeskEmployee")
            {
                // Optionally fetch existing managed employees if needed
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
        return View(employee);
    }

    public async Task<IActionResult> Delete(string id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        // Get the currently logged-in user's ID from session
        var currentUserId = HttpContext.Session.GetString("UserId");

        if (currentUserId == id)
        {
            // Optionally, add a message to TempData to show in the view
            TempData["ErrorMessage"] = "You cannot delete your own account while logged in.";
            return RedirectToAction("Index");
        }

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee != null)
            await _employeeService.DeleteEmployeeAsync(employee);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ManagedEmployees(string id)
    {
        var managedEmployees = await _employeeService.GetEmployeesManagedByAsync(id);
        // Ensure managedEmployees is never null
        return View(managedEmployees ?? new List<Employee>());
    }
}
