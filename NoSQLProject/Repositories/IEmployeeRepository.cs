using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByCredentialsAsync(string login, string password);

        Task<Employee?> GetByIdAsync(string id);
    }
}
