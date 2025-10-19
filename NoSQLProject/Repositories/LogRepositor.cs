using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    //  Added by TAREK — Sorting functionality for Logs page (Assignment 2)
    // This repository handles fetching and sorting log entries.
    // Implements MongoDB aggregation to sort logs dynamically by any field (default: CreatedAt DESC).
    public class LogRepository
    {
        private readonly IMongoCollection<Log> _logs;

        public LogRepository(IMongoDatabase db)
        {
            _logs = db.GetCollection<Log>("logs");
        }

        //  Default: Sort logs by CreatedAt descending (most recent first)
        // Uses MongoDB aggregation functions as required by the assignment.
        public async Task<List<Log>> GetAllSortedAsync(string sortField = "CreatedAt", int sortOrder = -1)
        {
            //  Added by TAREK — debug log to confirm method execution
            Console.WriteLine($"[TAREK] Sorting Logs by {sortField} ({(sortOrder == 1 ? "ASC" : "DESC")}) using aggregation");

            var sortStage = new BsonDocument("$sort", new BsonDocument(sortField, sortOrder));
            var pipeline = new[] { sortStage };

            return await _logs.Aggregate<Log>(pipeline).ToListAsync();
        }
    }
}
