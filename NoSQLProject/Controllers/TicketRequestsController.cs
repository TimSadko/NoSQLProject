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
        public async Task<IActionResult> Received()
        {
            var logged_in_employee = Authenticate();
            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                var requests = await _service.GetReceivedTicketRequestsAsync(logged_in_employee.Id);
                return View(requests);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<TicketRequest>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Sent()
        {
            var logged_in_employee = Authenticate();
            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                var requests = await _service.GetSentTicketRequestsAsync(logged_in_employee.Id);
                return View(requests);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<TicketRequest>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var logged_in_employee = Authenticate();
            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                var requests = await _service.GetAllTicketRequestsAsync();
                return View(requests);
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
                var view_model = new AddTicketRequestViewModel { TicketId = ticket_id };
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
                var request = await _service.GetRequestForDeleteAsync(request_id, logged_in_employee.Id);
                return View(request);
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
