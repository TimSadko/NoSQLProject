using NoSQLProject.Models;
using NoSQLProject.Repositories;

namespace NoSQLProject.Services
{
    public class TicketRequestService : ITicketRequestService
    {
        private readonly ITicketRequestRepository _rep;
        private readonly ITicketRepository _ticket_rep;
        private readonly IEmployeeRepository _employees_rep;

        public TicketRequestService(ITicketRequestRepository rep, ITicketRepository ticket_rep, IEmployeeRepository employees_rep)
        {
            _rep = rep;
            _ticket_rep = ticket_rep;
            _employees_rep = employees_rep;
        }

        // ========================= GET RECEIVED ============================
        public async Task<List<TicketRequest>> GetReceivedTicketRequestsAsync(string employee_id)
        {
            var ticket_requests = await _rep.GetAllByRecipientAsync(employee_id);

            var employees_tasks = new List<Task<Employee?>>();
            var ticket_tasks = new List<Task<Ticket?>>();

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

            return ticket_requests;
        }

        // ========================= GET SENT ============================
        public async Task<List<TicketRequest>> GetSentTicketRequestsAsync(string employee_id)
        {
            var ticket_requests = await _rep.GetAllBySenderAsync(employee_id);

            var employees_tasks = new List<Task<Employee?>>();
            var ticket_tasks = new List<Task<Ticket?>>();

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

            return ticket_requests;
        }

        // ========================= GET ALL ============================
        public async Task<List<TicketRequest>> GetAllTicketRequestsAsync()
        {
            var ticket_requests = await _rep.GetAllAsync();

            var employees_tasks = new List<Task<Employee?>>();
            var ticket_tasks = new List<Task<Ticket?>>();

            // Recipient
            for (int i = 0; i < ticket_requests.Count; i++)
            {
                employees_tasks.Add(_employees_rep.GetByIdAsync(ticket_requests[i].RecipientId));
                ticket_tasks.Add(_ticket_rep.GetByIdAsync(ticket_requests[i].TicketId));
            }

            // Sender
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

            return ticket_requests;
        }

        // ========================= ADD REQUEST ============================
        public async Task AddRequestAsync(string email, string logged_in_employee_id, string ticket_id, string message)
        {
            var emp = await _employees_rep.GetByEmailAsync(email);
            if (emp == null) 
                throw new Exception($"Employee with email \"{email}\" does not exist.");

            if (emp.Id == logged_in_employee_id)
                throw new Exception("You cannot send a ticket request to yourself.");

            if (emp is not ServiceDeskEmployee)
                throw new Exception("Only service desk employees can receive ticket requests.");

            var request = new TicketRequest
            {
                TicketId = ticket_id,
                Message = message ?? "",
                SenderId = logged_in_employee_id,
                RecipientId = emp.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var request = new TicketRequest();

            if (request.Message == null) request.Message = "";

            request.TicketId = ticket_id;
            request.Message = message;

            request.SenderId = logged_in_employee_id;
            request.RecipientId = emp.Id;

            List<Task> tasks = new List<Task>();

            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;        

            // Request redirection part
            List<TicketRequest> ticket_requests = await _rep.GetRequestsByTicketAsync(ticket_id);

            foreach (TicketRequest r in ticket_requests)
            {
                if (r.RecipientId == logged_in_employee_id && (r.Status == TicketRequestStatus.Open || r.Status == TicketRequestStatus.Accepted)) tasks.Add(_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Redirected));
            }

			tasks.Add(_rep.AddAsync(request));
<<<<<<<<< Temporary merge branch 1
			// Request redirection part
			List<TicketRequest> ticket_requests = await _rep.GetRequestsByTicketAsync(ticket_id);

			foreach (TicketRequest r in ticket_requests)
			{
				if (r.RecipientId == logged_in_employee_id && (r.Status == TicketRequestStatus.Open || r.Status == TicketRequestStatus.Accepted)) tasks.Add(_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Redirected));
			}
=========
            foreach (TicketRequest r in ticket_requests)
            {
                if (r.RecipientId == logged_in_employee_id && (r.Status == TicketRequestStatus.Open || r.Status == TicketRequestStatus.Accepted))
                    tasks.Add(_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Redirected));
            }

			await Task.WhenAll(tasks);
        }

        // ========================= VIEW PAGE ============================
        public async Task<(string, TicketRequest)> GetViewPageAsync(string request_id, string logged_in_employee_id)
        {
            var request = await _rep.GetByIdAsync(request_id)
                ?? throw new Exception("Request not found.");

            var recipient = await _employees_rep.GetByIdAsync(request.RecipientId)
                ?? throw new Exception("Recipient not found.");

            var sender = await _employees_rep.GetByIdAsync(request.SenderId)
                ?? throw new Exception("Sender not found.");

            var ticket = await _ticket_rep.GetByIdAsync(request.TicketId)
                ?? throw new Exception("Ticket not found.");

            request.Sender = sender;
            request.Recipient = recipient;
            request.Ticket = ticket;

            if (logged_in_employee_id == request.SenderId)
            if (logged_in_employee_id != request.SenderId) throw new Exception("Page inaccessible! Log in as a sender to delete the request");

            if (logged_in_employee_id == request.RecipientId)
                return ("ViewRecipient", request);

            return ("ViewGuest", request);
        }

        // ========================= GET DELETE PAGE ============================
        public async Task<TicketRequest> GetRequestForDeleteAsync(string request_id, string logged_in_employee_id)
        {
            if (logged_in_employee_id != loaded_request.SenderId) throw new Exception("Page inaccessible! Log in as a sender to delete the request");
                ?? throw new Exception("Request not found.");

            if (logged_in_employee_id != request.SenderId) throw new Exception("Page unaccessable! Log in as a sender to delete the request");

            return request;
        }

        // ========================= DELETE REQUEST ============================
        public async Task DeleteRequestAsync(string request_id, string logged_in_employee_id)
        {
            TicketRequest? request = await _rep.GetByIdAsync(request_id);

            if (request == null) throw new Exception("Ticket request with the id do not exists");

            if (request == null) throw new Exception("Ticket request with the id do not exists");
            if (req.Status != TicketRequestStatus.Open)
            if (request.Status == condition_status) await _rep.UpdateRequestStatusAsync(request_id, set_status);
        }
	  }	
}
        // ========================= CHANGE STATUS ============================
        public async Task ChangeRequestStausOnConditionAsync(string request_id, TicketRequestStatus condition_status, TicketRequestStatus set_status)
        {
            TicketRequest? request = await _rep.GetByIdAsync(request_id);

            if (request == null) throw new Exception("Ticket request with the id do not exsists");

<<<<<<<<< Temporary merge branch 1
			if (request == null) throw new Exception("Ticket request with the id do not exsists");

			if (request.Status == condition_status) await _rep.UpdateRequestStatusAsync(request_id, set_status);
		}
	}	
}
=========
            if (request.Status == condition_status) await _rep.UpdateRequestStatusAsync(request_id, set_status);
        }
    }    
}
>>>>>>>>> Temporary merge branch 2
