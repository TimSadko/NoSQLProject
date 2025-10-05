using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;

namespace NoSQLProject.Controllers
{
    public class TicketsEmployeeController(ITicketRepository repository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedEmployee = Authenticate();
            if (authenticatedEmployee?.Id == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var tickets = await repository.GetAllByEmployeeIdAsync(authenticatedEmployee.Id);
            var employeeTickets = new EmployeeTickets(tickets, authenticatedEmployee);
            return View(employeeTickets);
        }

        public Employee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or ServiceDeskEmployee ? null : employee;
        }
    }
}
