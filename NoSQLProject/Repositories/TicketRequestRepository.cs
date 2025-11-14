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

		public async Task<List<TicketRequest>> GetAllSortedAsync(string sort_field, int sort_order)
		{
			SortDefinition<TicketRequest> sort = sort_order == 1 ? Builders<TicketRequest>.Sort.Ascending(sort_field) : Builders<TicketRequest>.Sort.Descending(sort_field);

			return await _requests.Find(new BsonDocument()).Sort(sort).ToListAsync();
		}

		public async Task<List<TicketRequest>> GetAllByRecipientAsync(string recipient_id)
        {
            return await _requests.FindAsync(Builders<TicketRequest>.Filter.Eq("recipient_id", recipient_id)).Result.ToListAsync();
        }

		public async Task<List<TicketRequest>> GetAllByRecipientSortedAsync(string sort_field, int sort_order, string recipient_id)
		{
			SortDefinition<TicketRequest> sort = sort_order == 1 ? Builders<TicketRequest>.Sort.Ascending(sort_field) : Builders<TicketRequest>.Sort.Descending(sort_field);
			
			return await _requests.Find(Builders<TicketRequest>.Filter.Eq("recipient_id", recipient_id)).Sort(sort).ToListAsync();
        }

        public async Task<List<TicketRequest>> GetAllBySenderAsync(string sender_id)
        {
            return await _requests.FindAsync(Builders<TicketRequest>.Filter.Eq("sender_id", sender_id)).Result.ToListAsync();
        }

		public async Task<List<TicketRequest>> GetAllBySenderSortedAsync(string sort_field, int sort_order, string sender_id)
		{
			SortDefinition<TicketRequest> sort = sort_order == 1 ? Builders<TicketRequest>.Sort.Ascending(sort_field) : Builders<TicketRequest>.Sort.Descending(sort_field);

			return await _requests.Find(Builders<TicketRequest>.Filter.Eq("sender_id", sender_id)).Sort(sort).ToListAsync();
		}

		public async Task<TicketRequest?> GetByIdAsync(string request_id)
        {
            return await _requests.FindAsync(Builders<TicketRequest>.Filter.Eq("_id", ObjectId.Parse(request_id))).Result.FirstOrDefaultAsync();
        }

        public async Task AddAsync(TicketRequest request)
        {
            await _requests.InsertOneAsync(request);
        }

        public async Task DeleteAsync(string request_id)
        {
            await _requests.DeleteOneAsync(Builders<TicketRequest>.Filter.Eq("_id", ObjectId.Parse(request_id)));
        }

        public async Task<List<TicketRequest>> GetRequestsByTicketAsync(string ticket_id)
        {         
            return await _requests.FindAsync(Builders<TicketRequest>.Filter.Where(r => r.TicketId == ticket_id)).Result.ToListAsync();
        }

        public async Task UpdateRequestStatusAsync(string request_id, TicketRequestStatus status)
        {
            await _requests.UpdateOneAsync(Builders<TicketRequest>.Filter.Eq("_id", ObjectId.Parse(request_id)), Builders<TicketRequest>.Update.Set("status", status).Set("updated_at", DateTime.UtcNow));
        }
    }
}
