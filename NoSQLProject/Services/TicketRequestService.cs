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

        public async Task<List<TicketRequest>> GetReceivedTicketRequestsAsync(string employee_id)
        {
            List<TicketRequest> ticket_requests = await _rep.GetAllByRecipientAsync(employee_id);

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

            return ticket_requests;
        }

        public async Task<List<TicketRequest>> GetSentTicketRequestsAsync(string employee_id)
        {
            List<TicketRequest> ticket_requests = await _rep.GetAllBySenderAsync(employee_id);

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

            return ticket_requests;
        }

        public async Task<List<TicketRequest>> GetAllTicketRequestsAsync()
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

            return ticket_requests;
        }

        public async Task AddRequestAsync(string email, string logged_in_employee_id, string ticket_id, string message)
        {
            var emp = await _employees_rep.GetByEmailAsync(email);

            if (emp == null) throw new Exception($"Employee with email: \"{email}\" does not exist, please enter valid service desk employee email address");

            if (emp.Id == logged_in_employee_id) throw new Exception($"You cannot sent ticket request to yourself");

            if (emp is not ServiceDeskEmployee) throw new Exception($"You can sent ticket request only to service desk employees");

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

            await Task.WhenAll(tasks);
        }

        public async Task<(string, TicketRequest)> GetViewPageAsync(string request_id, string logged_in_employee_id)
        {
            TicketRequest? request = await _rep.GetByIdAsync(request_id);

            if (request == null) throw new Exception("Could not find request with the id");

            Employee? recipient = await _employees_rep.GetByIdAsync(request.RecipientId);

            if (recipient == null) throw new Exception("Could not find ticket recipient with the id");

            Employee? sender = await _employees_rep.GetByIdAsync(request.SenderId);
            if (sender == null) throw new Exception("Could not find ticket sender with the id");

            Ticket? ticket = await _ticket_rep.GetByIdAsync(request.TicketId);
            if (ticket == null) throw new Exception("Could not find request ticket with the id");

            request.Sender = sender;
            request.Recipient = recipient;
            request.Ticket = ticket;

            if (logged_in_employee_id == request.SenderId) return ("Edit", request);
            else if (logged_in_employee_id == request.RecipientId) return ("ViewRecipient", request);
            else return ("ViewGuest", request);
        }

        public async Task<TicketRequest> GetRequestForDeleteAsync(string request_id, string logged_in_employee_id)
        {
            TicketRequest? request = await _rep.GetByIdAsync(request_id);

            if (request == null) throw new Exception("Could not found request with the id");

            if (logged_in_employee_id != request.SenderId) throw new Exception("Page inaccessible! Log in as a sender to delete the request");

            return request;
        }

        public async Task DeleteRequestAsync(string request_id, string logged_in_employee_id)
        {
            TicketRequest? loaded_request = await _rep.GetByIdAsync(request_id);

            if (loaded_request == null) throw new Exception("Could not found request with the id");

            if (logged_in_employee_id != loaded_request.SenderId) throw new Exception("Page inaccessible! Log in as a sender to delete the request");

            if (loaded_request.Status != TicketRequestStatus.Open) throw new Exception("Only unaccepted requests could be deleted");

            await _rep.DeleteAsync(loaded_request.Id);
        }

        public async Task ChangeRequestStausOnConditionAsync(string request_id, TicketRequestStatus condition_status, TicketRequestStatus set_status)
        {
            TicketRequest? request = await _rep.GetByIdAsync(request_id);

            if (request == null) throw new Exception("Ticket request with the id do not exists");

            if (request == null) throw new Exception("Ticket request with the id do not exists");

            if (request.Status == condition_status) await _rep.UpdateRequestStatusAsync(request_id, set_status);
        }
	  }	
}
