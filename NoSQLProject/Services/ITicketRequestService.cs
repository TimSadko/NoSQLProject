using NoSQLProject.Models;

namespace NoSQLProject.Services
{
	public interface ITicketRequestService
	{
		Task<List<TicketRequest>> GetReceivedTicketRequestsAsync(string sort_field, int sort_order, string employee_id);
		Task<List<TicketRequest>> GetSentTicketRequestsAsync(string sort_field, int sort_order, string employee_id);
		Task<List<TicketRequest>> GetAllTicketRequestsAsync(string sort_field, int sort_order);
		Task AddRequestAsync(string email, string logged_in_employee_id, string ticket_id, string message);
		Task<(string, TicketRequest)> GetViewPageAsync(string request_id, string logged_in_employee_id);
		Task<TicketRequest> GetRequestForDeleteAsync(string request_id, string logged_in_employee_id);
		Task DeleteRequestAsync(string request_id, string logged_in_employee_id);
		Task ChangeRequestStausOnConditionAsync(string request_id, TicketRequestStatus condition_status, TicketRequestStatus set_status);
	}
}
