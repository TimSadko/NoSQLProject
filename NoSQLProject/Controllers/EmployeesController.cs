using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Services;

namespace NoSQLProject.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeService _employeeService;

        public EmployeesController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortField = "Status", int sortOrder = 1, string status = "All", string role = "All")
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                // Always fetch a sorted list first so sorting remains consistent
                var employees = await _employeeService.GetAllEmployeesSortedAsync(sortField, sortOrder);

                // Filter by status if requested ("All" means no filter)
                if (!string.IsNullOrEmpty(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                {
                    // Try parse as enum name first (case-insensitive), fall back to numeric parsing
                    if (Enum.TryParse<Employee_Status>(status, true, out var enumStatus))
                    {
                        employees = employees.Where(e => e.Status == enumStatus).ToList();
                    }
                    else if (int.TryParse(status, out var statusInt))
                    {
                        employees = employees.Where(e => (int)e.Status == statusInt).ToList();
                    }
                }

                // Filter by role if requested ("All" means no filter)
                if (!string.IsNullOrEmpty(role) && !string.Equals(role, "All", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(role, "ServiceDeskEmployee", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(role, "ServiceDesk", StringComparison.OrdinalIgnoreCase))
                    {
                        employees = employees.Where(e => e is ServiceDeskEmployee).ToList();
                    }
                    else if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "Normal", StringComparison.OrdinalIgnoreCase))
                    {
                        employees = employees.Where(e => !(e is ServiceDeskEmployee)).ToList();
                    }
                }

                // ===================== SORTING LOGIC =====================
                employees = sortField switch
                {
                    "FirstName" => sortOrder == 1 ? employees.OrderBy(e => e.FirstName).ToList() : employees.OrderByDescending(e => e.FirstName).ToList(),
                    "LastName" => sortOrder == 1 ? employees.OrderBy(e => e.LastName).ToList() : employees.OrderByDescending(e => e.LastName).ToList(),
                    "Email" => sortOrder == 1 ? employees.OrderBy(e => e.Email).ToList() : employees.OrderByDescending(e => e.Email).ToList(),
                    "Status" => sortOrder == 1 ? employees.OrderBy(e => e.Status).ToList() : employees.OrderByDescending(e => e.Status).ToList(),
                    "Role" => sortOrder == 1 ? employees.OrderBy(e => e.GetType().Name).ToList() : employees.OrderByDescending(e => e.GetType().Name).ToList(),
                    _ => employees.OrderBy(e => e.FirstName).ToList()
                };

                // Debug logs (optional) 
                /*Console.WriteLine($"SORT FIELD = {sortField}, ORDER = {sortOrder}");
                if (employees.Count > 0)
                {
                    Console.WriteLine($"Top result after sorting: {employees[0].FirstName} {employees[0].LastName}");
                }*/

                // ========================================================== 

                ViewBag.Status = status;
                ViewBag.SortField = sortField;
                ViewBag.SortOrder = sortOrder;
                ViewBag.Status = status;
                ViewBag.Role = role;

                return View(employees);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return View(new List<Employee>());
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

                Employee toAdd = Role == "ServiceDeskEmployee"
                    ? new ServiceDeskEmployee
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Email = employee.Email,
                        Password = employee.Password,
                        Status = employee.Status
                    }
                    : employee;

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
                var existing = await _employeeService.GetEmployeeByIdAsync(employee.Id);
                if (existing == null) throw new Exception("Employee not found");

                if (string.IsNullOrWhiteSpace(employee.Password))
                {
                    employee.Password = existing.Password;
                    ModelState.Remove("Password");
                    ModelState.Remove("employee.Password");
                }

                if (!ModelState.IsValid) throw new Exception("The model is invalid");

                Employee toUpdate;

                if (Role == "ServiceDeskEmployee")
                {
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
        public async Task<IActionResult> ManagedEmployees(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                var managedEmployees = await _employeeService.GetEmployeesManagedByAsync(id);
                var allEmployees = (await _employeeService.GetAllEmployeesAsync()).ToList();
                var managedIds = managedEmployees?.Select(e => e.Id).ToHashSet() ?? new HashSet<string>();
                var allManagedIds = await _employeeService.GetAllManagedEmployeeIdsAsync();

                var availableToAdd = allEmployees
                    .Where(e => !(e is ServiceDeskEmployee) && !managedIds.Contains(e.Id) && !allManagedIds.Contains(e.Id))
                    .ToList();

                ViewBag.AvailableEmployees = availableToAdd;
                ViewBag.ServiceDeskId = id;

                return View(managedEmployees ?? new List<Employee>());
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return View(new List<Employee>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManagedEmployee(string serviceDeskId, string employeeId)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _employeeService.AddManagedEmployeeAsync(serviceDeskId, employeeId);
                return RedirectToAction("ManagedEmployees", new { id = serviceDeskId });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("ManagedEmployees", new { id = serviceDeskId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveManagedEmployee(string serviceDeskId, string employeeId)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _employeeService.RemoveManagedEmployeeAsync(serviceDeskId, employeeId);
                return RedirectToAction("ManagedEmployees", new { id = serviceDeskId });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("ManagedEmployees", new { id = serviceDeskId });
            }
        }

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(HttpContext);
            return emp is ServiceDeskEmployee; // Only ServiceDeskEmployee can access
        }

		/*[HttpGet]
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
					throw new Exception("You cannot delete your own account.");

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
		}*/
	}
}
