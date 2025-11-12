using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRequestRepository
    {
        // ✅ Get all requests (optionally include archived)
        Task<List<TicketRequest>> GetAllAsync(bool allowArchived = false);

        // ✅ Get all requests assigned to a specific recipient (e.g., manager)
        Task<List<TicketRequest>> GetAllByRecipientAsync(string recipientId, bool allowArchived = false);

        // ✅ Get all requests sent by a specific employee
        Task<List<TicketRequest>> GetAllBySenderAsync(string senderId, bool allowArchived = false);

        // ✅ Get a single request by its ID
        Task<TicketRequest?> GetByIdAsync(string requestId);

        // ✅ Add a new request
        Task AddAsync(TicketRequest request);

        // ✅ Delete a request by its ID
        Task DeleteAsync(string requestId);

        // ✅ Get all requests related to a specific ticket
        Task<List<TicketRequest>> GetTicketRequestsAsync(string ticketId);

        // ✅ Update a request’s status (e.g., Pending → Approved / Denied)
        Task UpdateRequestStatusAsync(string requestId, TicketRequestStatus status);
    }
}
