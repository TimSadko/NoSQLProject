using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;

namespace NoSQLProject.Controllers
{
    public class TicketsEmployeeController : Controller
    {
        private readonly ITicketRepository _repository;

        public TicketsEmployeeController(ITicketRepository repository)
        {
            _repository = repository;
        }

        // ✅ Index: Displays tickets with optional sorting
        [HttpGet]
        public async Task<IActionResult> Index(string sortField = "CreatedAt", int sortOrder = -1)
        {
            var authenticatedEmployee = Authenticate();
            if (authenticatedEmployee?.Id == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // ✅ Get all tickets sorted, then filter by this employee
            var allTickets = await _repository.GetAllSortedAsync(sortField, sortOrder);
            var employeeTicketsList = allTickets
                .Where(t => t.CreatedById == authenticatedEmployee.Id)
                .ToList();

            var employeeTickets = new EmployeeTickets(employeeTicketsList, authenticatedEmployee);
            return View(employeeTickets);
        }

        // ✅ GET: Add ticket page
        [HttpGet]
        public IActionResult Add()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }
            return View(new Ticket());
        }

        // ✅ POST: Add new ticket
        [HttpPost]
        public async Task<IActionResult> Add(Ticket ticket)
        {
            var authenticatedEmployee = Authenticate();

            if (authenticatedEmployee?.Id == null)
                return RedirectToAction("Login", "Home");

            try
            {
                ticket.CreatedById = authenticatedEmployee.Id;
                ticket.Status = Ticket_Status.Open;
                ticket.Logs = new List<Log>();
                ticket.CreatedAt = DateTime.Now;
                ticket.UpdatedAt = DateTime.Now;

                await _repository.AddAsync(ticket);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(ticket);
            }
        }

        // ✅ Authentication helper
        private Employee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or ServiceDeskEmployee ? null : employee;
        }

        private bool IsAuthenticated() => Authenticate() != null;
    }
}
