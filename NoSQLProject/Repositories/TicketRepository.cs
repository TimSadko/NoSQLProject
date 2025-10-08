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

        public async Task<Ticket?> GetByIdAsync(string id)
        {
            if(string.IsNullOrEmpty(id)) return null;

            return await _tickets.FindAsync(Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(id))).Result.FirstOrDefaultAsync();
        }

        public async Task AddAsync(Ticket t)
        {
            await _tickets.InsertOneAsync(t);
        }

        public async Task EditAsync(Ticket t)
        {
            t.UpdatedAt = DateTime.Now;

            await _tickets.ReplaceOneAsync(Builders<Ticket>.Filter.Eq("_id", t.Id), t);
        }

        public async Task CheckUpdateAsync(Ticket t)
        {
            var old = await GetByIdAsync(t.Id); // Get 'db' version pf the ticket, then compare it to the 'edited' version

            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(t.Id)); // Get the Ticket by id

            bool change = false;

            List<Task<UpdateResult>> tsk = new List<Task<UpdateResult>>(); // Create list of Tasks

            if(old.Title != t.Title) // If title was changed, update it in the db
            {
                change = true;

                tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("title", t.Title))); 
            }

            if(old.Description != t.Description) // If description was changed, update it in the db
            {
                change = true;

                tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("description", t.Description)));
            }

            if (old.Status != t.Status) // If status was changed, update it in the db
            {
                change = true;

                tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("status", t.Status)));
            }

            if(change) tsk.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("updated_at", DateTime.Now))); // If any change was made, the 'updated_at' is set to now

            await Task.WhenAll(tsk); // wait for all of the updates to finish
        }

        public async Task AddLogAsync(Ticket t, Log l, Employee e)
        {
            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(t.Id)); // Create filter for the Ticket

            List<Task<UpdateResult>> tasks = new List<Task<UpdateResult>>(); // Create list of task, so they can be done in parallel

            l.Id = ObjectId.GenerateNewId().ToString();
            l.CreatedAt = DateTime.Now;
            l.CreatedById = e.Id; 

            tasks.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Push(ticket => ticket.Logs, l))); // Add log to the ticket

            tasks.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("status", l.NewStatus))); // Update the ticket status 

            tasks.Add(_tickets.UpdateOneAsync(filter, Builders<Ticket>.Update.Set("updated_at", DateTime.Now))); // Update the ticket last update time 

            await Task.WhenAll(tasks);
        }

        public async Task DeleteAsync(string id)
        {
            await _tickets.DeleteOneAsync(Builders<Ticket>.Filter.Eq("_id", id));
        }
    }
}
