using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;

namespace NoSQLProject.Controllers
{
    public class TicketsEmployeeController(ITicketRepository ticketRepository, IEmployeeRepository employeeRepository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedEmployee = Authenticate();
            if (authenticatedEmployee?.Id == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var tickets = await ticketRepository.GetAllByEmployeeIdAsync(authenticatedEmployee.Id);
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
                await ticketRepository.AddAsync(ticket);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(ticket);
            }
        }
        
        [HttpGet("TicketsEmployee/Logs/{id}")]
        public async Task<IActionResult> Logs(string? id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Home");

            try
            {
                if (string.IsNullOrEmpty(id)) throw new Exception("Ticket id is empty or null!");
                var logs = await ticketRepository.GetLogsByTicketIdAsync(id);

                if (logs.Count == 0)
                {
                    TempData["Exception"] = "No logs found for this ticket.";
                    return RedirectToAction("Index");
                }

                List<Tuple<Log, Employee>> employeeLogPairs = [];
                foreach (var log in logs)
                {
                    var employee = await employeeRepository.GetByIdAsync(log.CreatedById);
                    if (employee != null)
                    {
                        employeeLogPairs.Add(new Tuple<Log, Employee>(log, employee));
                    }
                }

                var viewModel = new TicketLogsViewModel(employeeLogPairs);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
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