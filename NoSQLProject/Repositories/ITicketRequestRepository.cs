using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRequestRepository
    {
        Task<List<TicketRequest>> GetAllAsync(bool allowArchived = false);
        Task<List<TicketRequest>> GetAllByRecipientAsync(string recipientId, bool allowArchived = false);
        Task<List<TicketRequest>> GetAllBySenderAsync(string senderId, bool allowArchived = false);
        Task<TicketRequest?> GetByIdAsync(string requestId);
        Task AddAsync(TicketRequest request);
        Task DeleteAsync(string requestId);
        Task<List<TicketRequest>> GetRequestsByTicketAsync(string ticketId);
        Task UpdateRequestStatusAsync(string requestId, TicketRequestStatus status);
    }
}
