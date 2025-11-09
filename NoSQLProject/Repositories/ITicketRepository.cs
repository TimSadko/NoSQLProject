using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync();
        Task<List<Ticket>> GetAllByEmployeeIdAsync(string id);
        Task<Ticket?> GetByIdAsync(string id);
        Task AddAsync(Ticket t);
        Task EditAsync(Ticket t);
        Task DeleteAsync(string id);
        Task CheckUpdateAsync(Ticket t);
        Task<List<Ticket>> GetAllSortedAsync(string sortField = "CreatedAt", int sortOrder = -1);
        Task AddLogAsync(Ticket t, Log l, Employee e);
        Task<Log?> GetLogByIdAsync(string ticket_id, string log_id);
        Task EditLogAsync(string ticket_id, Log log);
        Task DeleteLogAsync(string ticket_id, string log_id);
        Task<List<Log>> GetLogsByTicketIdAsync(string id);

        // ✅ NEW: Method to set default priority for existing tickets
        Task SetDefaultPriorityForNullRecordsAsync();
    }
}