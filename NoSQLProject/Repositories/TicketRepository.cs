using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;

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
            return await _tickets.FindAsync(Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(id))).Result.FirstOrDefaultAsync();
        }

        public async Task AddAsync(Ticket t)
        {
            await _tickets.InsertOneAsync(t);
        }

        public async Task EditAsync(Ticket t)
        {
            await _tickets.ReplaceOneAsync(Builders<Ticket>.Filter.Eq("_id", t.Id), t);
        }

        public async Task CheckUpdateAsync(Ticket t)
        {
            var old = await GetByIdAsync(t.Id); // Get 'db' version pf the ticket, then compare it to the 'edited' version

            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(t.Id)); // Get the Ticket by id

            List<Task<UpdateResult>> tsk = new List<Task<UpdateResult>>(); // Create list of Tasks

            if(old.Title != t.Title) // If title was changed, update it in the db
            {
                tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("title", t.Title))); 
            }

            if(old.Description != t.Description) // If description was changed, update it in the db
            {
                tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("description", t.Description)));
            }

            if (old.Status != t.Status) // If status was changed, update it in the db
            {
                tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("status", t.Status)));
            }

            await Task.WhenAll(tsk); // wait for all of the updates to finish
        }

        public async Task DeleteAsync(string id)
        {
            await _tickets.DeleteOneAsync(Builders<Ticket>.Filter.Eq("_id", id));
        }
    }
}
