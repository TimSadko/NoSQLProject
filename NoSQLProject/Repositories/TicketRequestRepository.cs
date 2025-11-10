using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public class TicketRequestRepository : ITicketRequestRepository
    {
        private readonly IMongoCollection<TicketRequest> _requests;

        public TicketRequestRepository(IMongoDatabase db)
        {
            _requests = db.GetCollection<TicketRequest>("ticket_requests");
        }

        public async Task<List<TicketRequest>> GetAllAsync()
        {
            return await _requests.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task<List<TicketRequest>> GetAllByRecipientAsync(string recipient_id, bool allow_archived = false)
        {
            if (!allow_archived) 
            {
                return await _requests.FindAsync(Builders<TicketRequest>.Filter.And(Builders<TicketRequest>.Filter.Eq("recipient_id", recipient_id), Builders<TicketRequest>.Filter.Eq("archived", false))).Result.ToListAsync();
            }

            return await _requests.FindAsync(Builders<TicketRequest>.Filter.Eq("recipient_id", recipient_id)).Result.ToListAsync();
        }

        public async Task AddAsync(TicketRequest request)
        {
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            await _requests.InsertOneAsync(request);
        }
    }
}
