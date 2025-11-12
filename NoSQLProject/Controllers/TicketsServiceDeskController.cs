using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.Services;

namespace NoSQLProject.Controllers
{
    public class TicketsServiceDeskController : Controller
    {
        private readonly IServiceDeskEmployeeService _service;
        private readonly ITicketRequestRepository _requestRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public TicketsServiceDeskController(
            IServiceDeskEmployeeService service,
            ITicketRequestRepository requestRepository,
            IEmployeeRepository employeeRepository)
        {
            _service = service;
            _requestRepository = requestRepository;
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortField = "Priority", int sortOrder = -1)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                var tickets = await _service.GetTicketsSortedAsync(sortField, sortOrder);
                ViewBag.SortField = sortField;
                ViewBag.SortOrder = sortOrder;
                return View(tickets);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<Ticket>());
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");
            return View(new Ticket());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket t)
        {
            var emp = Authorization.GetLoggedInEmployee(HttpContext);
            if (emp == null || emp is not ServiceDeskEmployee)
                return RedirectToAction("Login", "Home");

            try
            {
                await _service.AddTicketAsync(t, emp);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(t);
            }
        }

        [HttpGet("TicketsServiceDesk/Edit/{id}")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                return View(await _service.LoadTicketByIdAsync(id));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ticket ticket)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _service.EditTicketAsync(ticket);
                return RedirectToAction("Edit", new { id = ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("TicketsServiceDesk/AddLog/{id}")]
        public async Task<IActionResult> AddLog(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                Ticket? t = await _service.GetTicketByIdAsync(id);
                return View(new LogViewModel(t, new Log() { NewStatus = t.Status }));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLog(LogViewModel model)
        {
            var emp = Authorization.GetLoggedInEmployee(this.HttpContext);
            if (emp == null || emp is not ServiceDeskEmployee)
                return RedirectToAction("Login", "Home");

            try
            {
                await _service.AddLogAsync(model.Ticket, model.Log, emp);
                return RedirectToAction("Edit", new { id = model.Ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Edit", new { id = model.Ticket.Id });
            }
        }

        [HttpGet("TicketsServiceDesk/EditLog/{ticket_id}/{log_id}")]
        public async Task<IActionResult> EditLog(string ticket_id, string log_id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                return View(await _service.GetLogViewModelAsync(ticket_id, log_id));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditLog(LogViewModel view_model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _service.EditLogAsync(view_model.Ticket.Id, view_model.Log);
                return RedirectToAction("Edit", new { id = view_model.Ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                return View(await _service.LoadTicketByIdAsync(id));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Ticket ticket)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _service.DeleteTicketAsync(ticket.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("TicketsServiceDesk/DeleteLog/{ticket_id}/{log_id}")]
        public async Task<IActionResult> DeleteLog(string ticket_id, string log_id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                return View(await _service.GetLogViewModelAsync(ticket_id, log_id));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLog(LogViewModel view_model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _service.DeleteLogAsync(view_model.Ticket.Id, view_model.Log.Id);
                return RedirectToAction("Edit", new { id = view_model.Ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ✅ NEW: Escalate / Close Ticket
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string actionType)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                if (string.IsNullOrEmpty(id)) throw new Exception("Ticket ID is missing.");
                var emp = Authorization.GetLoggedInEmployee(HttpContext);
                if (emp == null || emp is not ServiceDeskEmployee) throw new Exception("Not authorized.");

                var ticket = await _service.GetTicketByIdAsync(id);
                if (ticket == null) throw new Exception("Ticket not found.");

                if (actionType == "escalate")
                {
                    // Change ticket status to Escalated
                    ticket.Status = Ticket_Status.Escalated;
                    await _service.EditTicketAsync(ticket);

                    // Create a TicketRequest to Management
                    var managers = await _employeeRepository.GetAllAsync();
                    var manager = managers.FirstOrDefault(e => !(e is ServiceDeskEmployee));
                    if (manager != null)
                    {
                        var request = new TicketRequest
                        {
                            TicketId = ticket.Id,
                            SenderId = emp.Id,
                            RecipientId = manager.Id,
                            Message = $"Ticket '{ticket.Title}' has been escalated for review.",
                            Status = TicketRequestStatus.Open,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _requestRepository.AddAsync(request);
                    }

                    TempData["Success"] = "Ticket escalated successfully and request sent to management.";
                }
                else if (actionType == "close")
                {
                    ticket.Status = Ticket_Status.Closed;
                    await _service.EditTicketAsync(ticket);
                    TempData["Success"] = "Ticket closed successfully.";
                }
                else
                {
                    throw new Exception("Unknown action type.");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        private bool Authenticate()
        {
            var emp = Authorization.GetLoggedInEmployee(HttpContext);
            return emp is ServiceDeskEmployee;
        }
    }
}
