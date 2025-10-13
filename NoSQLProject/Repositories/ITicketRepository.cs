using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(string id);
        Task AddAsync(Ticket t);
        Task EditAsync(Ticket t);
        Task DeleteAsync(string id);
        Task CheckUpdateAsync(Ticket t);
        Task AddLogAsync(Ticket t, Log l, Employee e);
        Task<Log?> GetLogByIdAsync(string ticket_id, string log_id);
        Task EditLogAsync(string ticket_id, Log log);
    }
}
