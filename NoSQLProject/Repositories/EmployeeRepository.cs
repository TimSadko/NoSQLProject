using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;
using NoSQLProject.Other; // For Hasher. Added by Fernando

namespace NoSQLProject.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;

        public EmployeeRepository(IMongoDatabase db)
        {
            _employees = db.GetCollection<Employee>("employees");
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _employees.FindAsync(Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(id))).Result.FirstOrDefaultAsync();
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

        // Add, Update, Delete methods added by Fernando
        public async Task Add(Employee employee)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Email, employee.Email);
            var existing = await _employees.Find(filter).FirstOrDefaultAsync();
            if (existing != null)
                throw new InvalidOperationException("An employee with this email already exists.");

            employee.Password = Hasher.GetHashedString(employee.Password);
            await _employees.InsertOneAsync(employee);
        }

        public async Task Update(Employee employee)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Id, employee.Id);
            await _employees.ReplaceOneAsync(filter, employee);
        }

        public async Task Delete(Employee employee)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Id, employee.Id);
            await _employees.DeleteOneAsync(filter);
        }
    }
}
