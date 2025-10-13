using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByCredentialsAsync(string login, string password);

        Task<Employee?> GetByIdAsync(string id);
        //Following Add, Update, Delete added by Fernando
        Task Add(Employee employee);
        Task Update(Employee employee);
        Task Delete(Employee employee);
        Task<Employee?> GetByEmailAsync(string email);
        Task<List<Employee>> GetEmployeesByIdsAsync(IEnumerable<string> ids);
        Task<List<Employee>> GetByStatusAggregationAsync(string status);
    }
}
