
using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface IEmployeeRepository
    {
        // ✅ Existing methods (Fernando)
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByCredentialsAsync(string login, string password);
        Task<Employee?> GetByIdAsync(string id);

        // Following Add, Update, Delete added by Fernando
        Task Add(Employee employee);
        Task Update(Employee employee);
        Task Delete(Employee employee);

        Task<Employee?> GetByEmailAsync(string email);
        Task<List<Employee>> GetEmployeesByIdsAsync(IEnumerable<string> ids);

        //  Added by TAREK — Sorting functionality for Employees page (Assignment 2)
        // This method retrieves all employees sorted dynamically by any field.
        Task<List<Employee>> GetAllSortedAsync(string sortField = "Status", int sortOrder = 1);
        
    }
}
