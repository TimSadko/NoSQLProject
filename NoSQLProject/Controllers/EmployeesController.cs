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

    // ✅✅✅ Added by TAREK — Sorting functionality for Employee page (Assignment 2)
    // Allows sorting by any visible field (default: Status then CreatedAt if available)
    [HttpGet]
    public async Task<IActionResult> Index(string sortField = "Status", int sortOrder = 1)
    {
        var employees = await _employeeService.GetAllEmployeesSortedAsync(sortField, sortOrder);
        return View(employees);
    }
    // ✅ END of TAREK’s sorting feature


    // Add new Employee
    public IActionResult Add() => View();

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

        return View(employee);
    }

    // Edit Employee
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

    // Delete Employee
    public async Task<IActionResult> Delete(string id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var currentUserId = HttpContext.Session.GetString("UserId");

        if (currentUserId == id)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account while logged in.";
            return RedirectToAction("Index");
        }

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee != null)
            await _employeeService.DeleteEmployeeAsync(employee);

        return RedirectToAction("Index");
    }

    // Managed Employees (for ServiceDesk employees)
    public async Task<IActionResult> ManagedEmployees(string id)
    {
        var managedEmployees = await _employeeService.GetEmployeesManagedByAsync(id);
        return View(managedEmployees ?? new List<Employee>());
    }
}
