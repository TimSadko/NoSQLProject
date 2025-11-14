using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;

namespace NoSQLProject.Controllers
{
    public class TicketsEmployeeController : Controller
    {
        private readonly ITicketRepository ticketRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public TicketsEmployeeController(
            ITicketRepository ticketRepository,
            ITicketRepository _ticketRepository,
            IEmployeeRepository _employeeRepository)
        {
            this.ticketRepository = ticketRepository;
            this._ticketRepository = _ticketRepository;
            this._employeeRepository = _employeeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortField = "Priority", int sortOrder = -1)
        {
            var authenticatedEmployee = Authenticate();
            if (authenticatedEmployee == null)
                return RedirectToAction("Login", "Home");

            var tickets = await ticketRepository.GetAllByEmployeeIdAsync(authenticatedEmployee.Id, false);

            // ---------------- SORTING ----------------
            switch (sortField)
            {
                case "Priority":
                    tickets = sortOrder == -1
                        ? tickets.OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt).ToList()
                        : tickets.OrderBy(t => t.Priority).ThenByDescending(t => t.CreatedAt).ToList();
                    break;

                case "Title":
                    tickets = sortOrder == -1
                        ? tickets.OrderByDescending(t => t.Title).ToList()
                        : tickets.OrderBy(t => t.Title).ToList();
                    break;

                case "Status":
                    tickets = sortOrder == -1
                        ? tickets.OrderByDescending(t => t.Status).ToList()
                        : tickets.OrderBy(t => t.Status).ToList();
                    break;

                case "UpdatedAt":
                    tickets = sortOrder == -1
                        ? tickets.OrderByDescending(t => t.UpdatedAt).ToList()
                        : tickets.OrderBy(t => t.UpdatedAt).ToList();
                    break;

                case "CreatedAt":
                    tickets = sortOrder == -1
                        ? tickets.OrderByDescending(t => t.CreatedAt).ToList()
                        : tickets.OrderBy(t => t.CreatedAt).ToList();
                    break;
            }

            /* ======================= DEBUG LOGS ==========================
            Console.WriteLine("========== TICKET SORT DEBUG ==========");
            Console.WriteLine($"SortField = {sortField}");
            Console.WriteLine($"SortOrder = {sortOrder}");

            if (tickets.Any())
            {
                var t = tickets[0];
                Console.WriteLine($"Top ticket AFTER sorting: {t.Title}");
                Console.WriteLine($"Priority: {t.Priority}, Status: {t.Status}");
                Console.WriteLine($"Created: {t.CreatedAt}, Updated: {t.UpdatedAt}");
            }
            else
            {
                Console.WriteLine("⚠ No tickets found.");
            }

            Console.WriteLine("========================================");
            // ================================================================== */

            var viewModel = new EmployeeTickets(tickets, authenticatedEmployee);

            ViewBag.SortField = sortField;
            ViewBag.SortOrder = sortOrder;

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            return View(new Ticket());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket ticket)
        {
            var authenticatedEmployee = Authenticate();
            if (authenticatedEmployee == null)
                return RedirectToAction("Login", "Home");

            try
            {
                ticket.CreatedById = authenticatedEmployee.Id;
                ticket.Status = Ticket_Status.Open;
                ticket.Logs = new List<Log>();
                ticket.CreatedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;
                ticket.Priority = Ticket_Priority.Undefined;

                await ticketRepository.AddAsync(ticket);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(ticket);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            if (string.IsNullOrEmpty(id))
                throw new Exception("Ticket id is empty or null!");

            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
                TempData["Exception"] = "Ticket is null. Something went wrong!";

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ticket? ticketToChange)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            try
            {
                if (ticketToChange == null || string.IsNullOrEmpty(ticketToChange.Id))
                    throw new Exception("Ticket id is empty or null!");

                var ticketToUpdate = await ticketRepository.GetByIdAsync(ticketToChange.Id);
                if (ticketToUpdate == null)
                {
                    TempData["Exception"] = "Ticket not found.";
                    return View(ticketToChange);
                }

                ticketToChange.Title = ticketToChange.Title;
                ticketToChange.Description = ticketToChange.Description;
                ticketToChange.UpdatedAt = DateTime.UtcNow;

                await _ticketRepository.EditAsync(ticketToUpdate);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            if (string.IsNullOrEmpty(id))
                throw new Exception("Ticket id is empty or null!");

            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                TempData["Exception"] = "Ticket is null. Something went wrong!";

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Ticket? ticket)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            try
            {
                if (ticket == null || string.IsNullOrEmpty(ticket.Id))
                    throw new Exception("Ticket id is empty or null!");

                var ticketToRemove = await ticketRepository.GetByIdAsync(ticket.Id);
                if (ticketToRemove == null)
                {
                    TempData["Exception"] = "Ticket not found.";
                    return View(ticket);
                }

                await ticketRepository.DeleteAsync(ticket.Id);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Archive(string? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            if (string.IsNullOrEmpty(id))
                throw new Exception("Ticket id is empty or null!");

            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                TempData["Exception"] = "Ticket is null. Something went wrong!";

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> Archive(Ticket? ticket)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            try
            {
                if (ticket == null || string.IsNullOrEmpty(ticket.Id))
                    throw new Exception("Ticket id is empty or null!");

                var ticketToRemove = await ticketRepository.GetByIdAsync(ticket.Id);
                if (ticketToRemove == null)
                {
                    TempData["Exception"] = "Ticket not found.";
                    return View(ticket);
                }

                await ticketRepository.ArchiveAsync(ticket.Id);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("TicketsEmployee/Logs/{id}")]
        public async Task<IActionResult> Logs(string? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Home");

            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Ticket id is empty or null!");

                var logs = await _ticketRepository.GetLogsByTicketIdAsync(id);

                if (!logs.Any())
                {
                    TempData["Exception"] = "No logs found for this ticket.";
                    return RedirectToAction("Index");
                }

                var employeeLogPairs = new List<Tuple<Log, Employee>>();

                foreach (var log in logs)
                {
                    var employee = await _employeeRepository.GetByIdAsync(log.CreatedById);
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
            return Authorization.GetLoggedInEmployee(HttpContext);
        }

        private bool IsAuthenticated() => Authenticate() != null;
    }
}