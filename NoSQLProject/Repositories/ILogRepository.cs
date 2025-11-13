using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    //  Added by TAREK — Interface for Logs Repository (Assignment 2)
    // Defines the contract for sorting and retrieving logs using MongoDB aggregation.
    public interface ILogRepository
    {
        //  Retrieves all logs sorted dynamically by any field.
        // Default sorting: CreatedAt descending (latest logs first).
        Task<List<Log>> GetAllSortedAsync(string sortField = "CreatedAt", int sortOrder = -1);
    }
}
