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

        public TicketsServiceDeskController(IServiceDeskEmployeeService service)
        {
            _service = service;

		}

        [HttpGet]
        public async Task<IActionResult> Index(string sortField = "Priority", int sortOrder = -1)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                var tickets = await _service.GetTicketsSortedAsync(sortField, sortOrder);
            
                ViewBag.SortField = sortField; // Pass sort info to view
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

			if (emp == null || emp is not ServiceDeskEmployee) return RedirectToAction("Login", "Home");

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

			if (emp == null || emp is not ServiceDeskEmployee) return RedirectToAction("Login", "Home");

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

        private bool Authenticate()
        {
            var emp = Authorization.GetLoggedInEmployee(HttpContext);
            return emp is ServiceDeskEmployee;
        }
    }
}