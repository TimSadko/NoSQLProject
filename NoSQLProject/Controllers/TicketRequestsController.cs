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

        [HttpGet]
        public async Task<IActionResult> Received()
        {
            var logged_in_employee = Authenticate();

            if (logged_in_employee == null) return RedirectToAction("Login", "Home");

            try
            {
                List<TicketRequest> ticket_requests = await _rep.GetAllByRecipientAsync(logged_in_employee.Id);

                List<Task<Employee?>> employees_tasks = new List<Task<Employee?>>();
                List<Task<Ticket?>> ticket_tasks = new List<Task<Ticket?>>();

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].SenderId));
                    ticket_tasks.Add(_ticket_rep.GetByIdAsync(ticket_requests[i].TicketId));
                }

                await Task.WhenAll(employees_tasks); 
                await Task.WhenAll(ticket_tasks); 

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    ticket_requests[i].Sender = employees_tasks[i].Result;
                    ticket_requests[i].Ticket = ticket_tasks[i].Result;
                }

                return View(ticket_requests);
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
                List<TicketRequest> ticket_requests = await _rep.GetAllBySenderAsync(logged_in_employee.Id);

                List<Task<Employee?>> employees_tasks = new List<Task<Employee?>>();
                List<Task<Ticket?>> ticket_tasks = new List<Task<Ticket?>>();

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].RecipientId));
                    ticket_tasks.Add(_ticket_rep.GetByIdAsync(ticket_requests[i].TicketId));
                }

                await Task.WhenAll(employees_tasks);
                await Task.WhenAll(ticket_tasks);

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    ticket_requests[i].Recipient = employees_tasks[i].Result;
                    ticket_requests[i].Ticket = ticket_tasks[i].Result;
                }

                return View(ticket_requests);
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
                List<TicketRequest> ticket_requests = await _rep.GetAllAsync();

                List<Task<Employee?>> employees_tasks = new List<Task<Employee?>>();
                List<Task<Ticket?>> ticket_tasks = new List<Task<Ticket?>>();

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].RecipientId));
                    ticket_tasks.Add(_ticket_rep.GetByIdAsync(ticket_requests[i].TicketId));
                }

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].SenderId));
                }

                await Task.WhenAll(employees_tasks);
                await Task.WhenAll(ticket_tasks);

                for (int i = 0; i < ticket_requests.Count; i++)
                {
                    ticket_requests[i].Recipient = employees_tasks[i].Result;
                    ticket_requests[i].Sender = employees_tasks[i + ticket_requests.Count].Result;

                    ticket_requests[i].Ticket = ticket_tasks[i].Result;
                }

                return View(ticket_requests);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<TicketRequest>());
            }
        }

        [HttpGet("TicketRequests/Add/{ticket_id}")]
        public  ActionResult Add(string ticket_id)
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
                var emp = await _employees_rep.GetByEmailAsync(view_model.Email);

                if (emp == null) throw new Exception($"Employee with email \"{view_model.Email}\" does not exist, please enter valid service desk employee email");

                if(emp.Id == logged_in_employee.Id) throw new Exception($"You cannot sent ticket request to yourself");

                if(emp is not ServiceDeskEmployee) throw new Exception($"You can sent ticket request only to service desk employee");

                var request = new TicketRequest();

                if (request.Message == null) request.Message = "";

                request.TicketId = view_model.TicketId;
                request.Message = view_model.Message;

                request.SenderId = logged_in_employee.Id;
                request.RecipientId = emp.Id;

                await _rep.AddAsync(request);

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
                TicketRequest? request = await _rep.GetByIdAsync(request_id);

                if (request == null) throw new Exception("Could not find request with the id");
                
                Employee? recipient = await _employees_rep.GetByIdAsync(request.RecipientId);

                if(recipient == null) throw new Exception("Could not find ticket recipient with the id");

                Employee? sender = await _employees_rep.GetByIdAsync(request.SenderId);
                if (sender == null) throw new Exception("Could not find ticket sender with the id");

                Ticket? ticket = await _ticket_rep.GetByIdAsync(request.TicketId);
                if (ticket == null) throw new Exception("Could not find request ticket with the id");

                request.Sender = sender;
                request.Recipient = recipient;
                request.Ticket = ticket;

                if (logged_in_employee.Id == request.SenderId) return View("Edit", request);
                else if (logged_in_employee.Id == request.RecipientId) return View("ViewRecipient", request);
                else return View("ViewGuest", request);
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
				TicketRequest? request = await _rep.GetByIdAsync(request_id);

                if (request == null) throw new Exception("Could not found request with the id");

                if(logged_in_employee.Id != request.SenderId) throw new Exception("Page unaccessable! Log in as a sender to delete the request");

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
				TicketRequest? loaded_request = await _rep.GetByIdAsync(request_id_only.Id);

				if (loaded_request == null) throw new Exception("Could not found request with the id");

				if (logged_in_employee.Id != loaded_request.SenderId) throw new Exception("Page unaccessable! Log in as a sender to delete the request");

                await _rep.DeleteAsync(loaded_request.Id);

				return RedirectToAction("Sent");
			}
			catch (Exception ex)
			{
				ViewData["Exception"] = ex.Message;
				return RedirectToAction("Received");
			}
		}

        private ServiceDeskEmployee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or not ServiceDeskEmployee ? null : (ServiceDeskEmployee)employee;
        }

        public static string CutString(int max_length, string str)
        {
            if(string.IsNullOrEmpty(str)) return "-";

            if(str.Length > max_length)
            {
                return $"{str.Substring(0, max_length)}...";
            }
            
            return str;
        }
    }
}
