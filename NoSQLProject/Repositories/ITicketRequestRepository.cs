using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRequestRepository
    {
        Task<List<TicketRequest>> GetAllAsync();
        Task<List<TicketRequest>> GetAllByRecipientAsync(string recipient_id, bool allow_archived = false);
    }
}
