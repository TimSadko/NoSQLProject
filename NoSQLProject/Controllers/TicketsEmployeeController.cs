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

        [HttpGet]
        public IActionResult Add()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }
            return View(new Ticket());
        }
        
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
                ticket.Logs = [];
                ticket.CreatedAt = DateTime.Now;
                ticket.UpdatedAt = DateTime.Now;
                await repository.AddAsync(ticket);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(ticket);
            }
        }

        private Employee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or ServiceDeskEmployee ? null : employee;
        }
        
        private bool IsAuthenticated() => Authenticate() != null;
        
    }
}