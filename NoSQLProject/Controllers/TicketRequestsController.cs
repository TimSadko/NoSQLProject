using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Services;

namespace NoSQLProject.Controllers
{
    public class TicketRequestsController : Controller
    {
        private readonly ITicketRequestService _service;

        public TicketRequestsController(ITicketRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Received(string sortField = "UpdatedAt", int sortOrder = -1)
        {
            var logged_in_employee = Authenticate();
            
            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
				ViewBag.SortField = sortField;
				ViewBag.SortOrder = sortOrder;

				return View(await _service.GetReceivedTicketRequestsAsync(sortField, sortOrder, logged_in_employee.Id));
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<TicketRequest>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Sent(string sortField = "UpdatedAt", int sortOrder = -1)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
				ViewBag.SortField = sortField;
				ViewBag.SortOrder = sortOrder;

				return View(await _service.GetSentTicketRequestsAsync(sortField, sortOrder, logged_in_employee.Id));
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<TicketRequest>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> All(string sortField = "UpdatedAt", int sortOrder = -1)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
				ViewBag.SortField = sortField;
				ViewBag.SortOrder = sortOrder;

				return View(await _service.GetAllTicketRequestsAsync(sortField, sortOrder));
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<TicketRequest>());
            }
        }

        [HttpGet("TicketRequests/Add/{ticket_id}")]
        public ActionResult Add(string ticket_id)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                var view_model = new AddTicketRequestViewModel();

                view_model.TicketId = ticket_id;

                return View(view_model);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddTicketRequestViewModel view_model)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                await _service.AddRequestAsync(view_model.Email, logged_in_employee.Id, view_model.TicketId, view_model.Message);

                return RedirectToAction("Index", "TicketsServiceDesk");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(view_model);
            }
        }

        [HttpGet("TicketRequests/View/{request_id}")]
        public async Task<ActionResult> View(string request_id)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                var (viewName, model) = await _service.GetViewPageAsync(request_id, logged_in_employee.Id);

                return View(viewName, model);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return RedirectToAction("Received");
            }
        }

        [HttpGet("TicketRequests/Delete/{request_id}")]
        public async Task<ActionResult> Delete(string request_id)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                return View(await _service.GetRequestForDeleteAsync(request_id, logged_in_employee.Id));
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return RedirectToAction("Received");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(TicketRequest request_id_only)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                await _service.DeleteRequestAsync(request_id_only.Id, logged_in_employee.Id);

                return RedirectToAction("Sent");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return RedirectToAction("Received");
            }
        }

        [HttpPost]
        public async Task<ActionResult> ViewAccept(TicketRequest request_id_only)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                await _service.ChangeRequestStausOnConditionAsync(request_id_only.Id, TicketRequestStatus.Open, TicketRequestStatus.Accepted);

                return RedirectToAction("View", new { request_id = request_id_only.Id });
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return RedirectToAction("View", new { request_id = request_id_only.Id });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ViewReject(TicketRequest request_id_only)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                await _service.ChangeRequestStausOnConditionAsync(request_id_only.Id, TicketRequestStatus.Open, TicketRequestStatus.Rejected);

                return RedirectToAction("View", new { request_id = request_id_only.Id });
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return RedirectToAction("View", new { request_id = request_id_only.Id });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ViewFail(TicketRequest request_id_only)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                await _service.ChangeRequestStausOnConditionAsync(request_id_only.Id, TicketRequestStatus.Accepted, TicketRequestStatus.Failed);

                return RedirectToAction("View", new { request_id = request_id_only.Id });
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return RedirectToAction("View", new { request_id = request_id_only.Id });
            }
        }

        private ServiceDeskEmployee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or not ServiceDeskEmployee ? null : (ServiceDeskEmployee)employee;
        }

        public static string CutString(int max_length, string str)
        {
            if (string.IsNullOrEmpty(str)) return "-";
            return str.Length > max_length ? $"{str[..max_length]}..." : str;
        }
    }
}
