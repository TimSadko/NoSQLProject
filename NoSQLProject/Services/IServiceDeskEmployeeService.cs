using NoSQLProject.Models;

namespace NoSQLProject.Services
{
	public interface IServiceDeskEmployeeService
	{
		Task<List<Ticket>> GetTicketsSortedAsync(string sortField, int sortOrder);
		Task AddTicketAsync(Ticket ticket, Employee creator);
		Task<Ticket> GetTicketByIdAsync(string id);
		Task<Ticket> LoadTicketByIdAsync(string? id);
		Task EditTicketAsync(Ticket ticket);
		Task AddLogAsync(Ticket ticket, Log log, Employee creator);
		Task<LogViewModel> GetLogViewModelAsync(string ticket_id, string log_id);
		Task EditLogAsync(string ticket_id, Log log);
		Task ArchiveTicketAsync(string ticket_id);
		Task DeleteTicketAsync(string ticket_id);
		Task DeleteLogAsync(string ticket_id, string log_id);
		Task<string> UpdateStatusAsync(string ticket_id, string action_type, string logged_in_employee_id);
	}
}
