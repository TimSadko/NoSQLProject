using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRequestRepository
    {
        Task<List<TicketRequest>> GetAllAsync(bool allow_archived = false);
        Task<List<TicketRequest>> GetAllByRecipientAsync(string recipient_id, bool allow_archived = false);
        Task<List<TicketRequest>> GetAllBySenderAsync(string sender_id, bool allow_archived = false);
        Task<TicketRequest?> GetByIdAsync(string request_id);
        Task AddAsync(TicketRequest request);
        Task DeleteAsync(string request_id);
        Task<List<TicketRequest>> GetTicketRequestsAsync(string ticket_id);
        Task UpdateRequestStatus(string request_id, TicketRequestStatus status);
	}
}
