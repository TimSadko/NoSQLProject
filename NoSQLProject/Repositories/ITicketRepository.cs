using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync(bool archived = false);
		Task<List<Ticket>> GetAllByEmployeeIdAsync(string id, bool allow_archived = true);
		Task<Ticket?> GetByIdAsync(string id);
        Task AddAsync(Ticket t);
        Task EditAsync(Ticket t);
        Task UpdateTicketStatusAsync(string ticket_id, Ticket_Status status);
		Task DeleteAsync(string ticket_id);
        Task SetArchiveAsync(string ticket_id, bool archive = true);
		Task<List<Ticket>> GetAllSortedAsync(string sortField = "CreatedAt", int sortOrder = -1, bool archived = false);
		Task AddLogAsync(Ticket t, Log l, Employee e);
        Task<Log?> GetLogByIdAsync(string ticket_id, string log_id);
        Task EditLogAsync(string ticket_id, Log log);
        Task DeleteLogAsync(string ticket_id, string log_id);
        Task<List<Log>> GetLogsByTicketIdAsync(string id);
    }
}