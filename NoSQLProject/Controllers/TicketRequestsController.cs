using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.Services;

namespace NoSQLProject.Controllers
{
    public class TicketRequestsController : Controller
    {
        private readonly ITicketRequestRepository _rep;
        private readonly ITicketRepository _ticket_rep;
        private readonly IEmployeeRepository _employees_rep;

        public TicketRequestsController(ITicketRequestRepository rep, ITicketRepository ticket_rep, IEmployeeRepository employees_rep)
        {
            _rep = rep;
            _ticket_rep = ticket_rep;
            _employees_rep = employees_rep;
        }

        [HttpGet("TicketRequests/Received/{recipient_id}")]
        public async Task<IActionResult> Received(string recipient_id)
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");
            if (logged_in_employee.Id != recipient_id)
            {
                TempData["Exception"] = "Unable to access users page, in order to acces the page login as owner of the page";
                return RedirectToAction("Logout", "Home");
            }

            try
            {
                List<TicketRequest> ticket_requests = await _rep.GetAllByRecipientAsync(recipient_id);

                List<Task<Employee?>> employees_tasks = new List<Task<Employee?>>();

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].RecipientId));
                }

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].SenderId));
                }

                await Task.WhenAll(employees_tasks); 

                for (int i = 0; i < employees_tasks.Count; i++)
                {
                    ticket_requests[i].Recipient = employees_tasks[i].Result;
                    ticket_requests[i].Sender = employees_tasks[i + employees_tasks.Count - 1].Result;
                }

                return View(ticket_requests);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return View();
            }
        }

        private ServiceDeskEmployee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or not ServiceDeskEmployee ? null : (ServiceDeskEmployee)employee;
        }
    }
}
