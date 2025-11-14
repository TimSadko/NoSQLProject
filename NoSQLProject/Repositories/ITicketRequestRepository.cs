using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface ITicketRequestRepository
    {
        Task<List<TicketRequest>> GetAllAsync();
        Task<List<TicketRequest>> GetAllSortedAsync(string sort_field, int sort_order);
		Task<List<TicketRequest>> GetAllByRecipientAsync(string recipientId);
        Task<List<TicketRequest>> GetAllByRecipientSortedAsync(string sort_field, int sort_order, string recipient_id);
		Task<List<TicketRequest>> GetAllBySenderAsync(string senderId);
        Task<List<TicketRequest>> GetAllBySenderSortedAsync(string sort_field, int sort_order, string sender_id);
		Task<TicketRequest?> GetByIdAsync(string requestId);
        Task AddAsync(TicketRequest request);
        Task DeleteAsync(string requestId);
        Task<List<TicketRequest>> GetRequestsByTicketAsync(string ticketId);
        Task UpdateRequestStatusAsync(string requestId, TicketRequestStatus status);
    }
}
