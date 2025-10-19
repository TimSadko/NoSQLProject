
using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByCredentialsAsync(string login, string password);
        Task<Employee?> GetByIdAsync(string id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Employee employee);
        Task<Employee?> GetByEmailAsync(string email);
        Task<List<Employee>> GetEmployeesByIdsAsync(IEnumerable<string> ids);
<<<<<<< HEAD

        //  Added by TAREK — Sorting functionality for Employees page (Assignment 2)
        // This method retrieves all employees sorted dynamically by any field.
=======
        Task<List<Employee>> GetByStatusAggregationAsync(string status);
>>>>>>> 2198fd4e205037c5592e0f4e2cd81e946b9e0874
        Task<List<Employee>> GetAllSortedAsync(string sortField = "Status", int sortOrder = 1);
    }
}
