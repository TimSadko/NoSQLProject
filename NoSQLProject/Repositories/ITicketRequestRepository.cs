using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRequestRepository
    {
        Task<List<TicketRequest>> GetAllAsync(bool allow_archived = false);
        Task<List<TicketRequest>> GetAllByRecipientAsync(string recipient_id, bool allow_archived = false);
        Task<List<TicketRequest>> GetAllBySenderAsync(string sender_id, bool allow_archived = false);
        Task AddAsync(TicketRequest request);
    }
}
