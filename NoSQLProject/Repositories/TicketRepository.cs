using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;
using System.Threading.Tasks;

namespace NoSQLProject.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public TicketRepository(IMongoDatabase db)
        {
            _tickets = db.GetCollection<Ticket>("tickets");
        }

        public async Task<List<Ticket>> GetAllAsync()
        {
            return await _tickets.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task<List<Ticket>> GetAllByEmployeeIdAsync(string id)
        {
            return await _tickets.FindAsync(Builders<Ticket>.Filter.Eq("created_by", id)).Result.ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            return await _tickets.FindAsync(Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(id)))
                                 .Result
                                 .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Ticket t)
        {
            await _tickets.InsertOneAsync(t);
        }

        public async Task EditAsync(Ticket t)
        {
            await _tickets.ReplaceOneAsync(Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(t.Id)), t);
        }

        public async Task AddLogAsync(Ticket t, Log l, Employee e)
        {
            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(t.Id));
            var update = Builders<Ticket>.Update.Push(ticket => ticket.Logs, l);
            await _tickets.UpdateOneAsync(filter, update);
        }

        // ✅ Escalate / Close ticket functionality
        public async Task UpdateTicketStatusAsync(string ticket_id, Ticket_Status status)
        {
            if (string.IsNullOrEmpty(ticket_id))
                throw new ArgumentException("Ticket ID cannot be null or empty", nameof(ticket_id));

            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(ticket_id));
            var update = Builders<Ticket>.Update
                .Set(t => t.Status, status)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            await _tickets.UpdateOneAsync(filter, update);
        }

        public async Task<Log?> GetLogByIdAsync(string ticket_id, string log_id)
        {
            Ticket? t = await GetByIdAsync(ticket_id);
            return t?.Logs.FirstOrDefault(log => log.Id == log_id);
        }

        public async Task EditLogAsync(string ticket_id, Log log)
        {
            var filter = Builders<Ticket>.Filter.And(
                Builders<Ticket>.Filter.Eq(t => t.Id, ticket_id),
                Builders<Ticket>.Filter.ElemMatch(t => t.Logs, l => l.Id == log.Id)
            );

            var update = Builders<Ticket>.Update
                .Set("logs.$.description", log.Description)
                .Set("logs.$.new_status", log.NewStatus)
                .Set("updated_at", DateTime.UtcNow);

            await _tickets.UpdateOneAsync(filter, update);
        }

        public async Task DeleteLogAsync(string ticket_id, string log_id)
        {
            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(ticket_id));
            var update = Builders<Ticket>.Update.PullFilter(t => t.Logs, Builders<Log>.Filter.Eq("_id", ObjectId.Parse(log_id)));
            await _tickets.UpdateOneAsync(filter, update);
        }

        public async Task<List<Log>> GetLogsByTicketIdAsync(string id)
        {
            var ticket = await GetByIdAsync(id);
            return ticket == null ? [] : ticket.Logs;
        }

        public async Task DeleteAsync(string id)
        {
            await _tickets.DeleteOneAsync(Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(id)));
        }

        public async Task<List<Ticket>> GetAllSortedAsync(string sortField = "CreatedAt", int sortOrder = -1)
        {
            var sortBuilder = Builders<Ticket>.Sort;
            SortDefinition<Ticket> sortDef;

            if (sortField == "Priority")
            {
                sortDef = sortOrder == 1
                    ? sortBuilder.Ascending(t => t.Priority).Descending(t => t.CreatedAt)
                    : sortBuilder.Descending(t => t.Priority).Descending(t => t.CreatedAt);
            }
            else
            {
                sortDef = sortOrder == 1
                    ? sortBuilder.Ascending(sortField)
                    : sortBuilder.Descending(sortField);
            }

            return await _tickets.Find(new BsonDocument()).Sort(sortDef).ToListAsync();
        }
    }
}
