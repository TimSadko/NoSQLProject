using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;

        public EmployeeRepository(IMongoDatabase db)
        {
            _employees = db.GetCollection<Employee>("employees");
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _employees.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task<Employee?> GetByCredentialsAsync(string login, string password)
        {
            var builder = Builders<Employee>.Filter;
            var fillter = builder.And(builder.Eq("email", login), builder.Eq("password", password));

            return await _employees.FindAsync(fillter).Result.FirstOrDefaultAsync();
        }
    }
}
