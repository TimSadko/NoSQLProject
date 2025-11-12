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

        public async Task<List<TicketRequest>> GetAllAsync(bool allow_archived = false)
        {
            if (!allow_archived)
            {
                return await _requests.FindAsync(
                    Builders<TicketRequest>.Filter.Eq("archived", false)
                ).Result.ToListAsync();
            }

            return await _requests.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task<List<TicketRequest>> GetAllByRecipientAsync(string recipient_id, bool allow_archived = false)
        {
            if (!allow_archived)
            {
                var filter = Builders<TicketRequest>.Filter.And(
                    Builders<TicketRequest>.Filter.Eq("recipient_id", recipient_id),
                    Builders<TicketRequest>.Filter.Eq("archived", false)
                );

                return await _requests.FindAsync(filter).Result.ToListAsync();
            }

            return await _requests.FindAsync(
                Builders<TicketRequest>.Filter.Eq("recipient_id", recipient_id)
            ).Result.ToListAsync();
        }

        public async Task<List<TicketRequest>> GetAllBySenderAsync(string sender_id, bool allow_archived = false)
        {
            if (!allow_archived)
            {
                var filter = Builders<TicketRequest>.Filter.And(
                    Builders<TicketRequest>.Filter.Eq("sender_id", sender_id),
                    Builders<TicketRequest>.Filter.Eq("archived", false)
                );

                return await _requests.FindAsync(filter).Result.ToListAsync();
            }

            return await _requests.FindAsync(
                Builders<TicketRequest>.Filter.Eq("sender_id", sender_id)
            ).Result.ToListAsync();
        }

        public async Task<TicketRequest?> GetByIdAsync(string request_id)
        {
            return await _requests.FindAsync(
                Builders<TicketRequest>.Filter.Eq("_id", ObjectId.Parse(request_id))
            ).Result.FirstOrDefaultAsync();
        }

        public async Task AddAsync(TicketRequest request)
        {
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            await _requests.InsertOneAsync(request);
        }

        public async Task DeleteAsync(string request_id)
        {
            await _requests.DeleteOneAsync(
                Builders<TicketRequest>.Filter.Eq("_id", ObjectId.Parse(request_id))
            );
        }

        public async Task<List<TicketRequest>> GetTicketRequestsAsync(string ticket_id)
        {
            var filter = Builders<TicketRequest>.Filter.Where(
                r => r.TicketId == ticket_id && r.Archived == false
            );
            return await _requests.FindAsync(filter).Result.ToListAsync();
        }

        // ✅ FIXED: method name now matches the interface
        public async Task UpdateRequestStatusAsync(string request_id, TicketRequestStatus status)
        {
            var filter = Builders<TicketRequest>.Filter.Eq("_id", ObjectId.Parse(request_id));
            var update = Builders<TicketRequest>.Update
                .Set("status", status)
                .Set("updated_at", DateTime.UtcNow);

            await _requests.UpdateOneAsync(filter, update);
        }
    }
}
