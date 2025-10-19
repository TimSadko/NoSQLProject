
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

        // ✅ Get employee by ID (Fernando)
        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _employees.FindAsync(Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(id)))
                                   .Result.FirstOrDefaultAsync();
        }

        // ✅ Get all employees (Fernando)
        public async Task<List<Employee>> GetAllAsync()
        {
            return await _employees.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        // ✅ Get by credentials (Fernando)
        public async Task<Employee?> GetByCredentialsAsync(string login, string password)
        {
            var builder = Builders<Employee>.Filter;
            var filter = builder.And(builder.Eq("email", login), builder.Eq("password", password));
            return await _employees.FindAsync(filter).Result.FirstOrDefaultAsync();
        }

        // ✅ Get by email (Fernando)
        public async Task<Employee?> GetByEmailAsync(string email)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Email, email);
            return await _employees.Find(filter).FirstOrDefaultAsync();
        }

        // ✅ CRUD methods added by Fernando
        public async Task Add(Employee employee)
        {
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

        public async Task<List<Employee>> GetEmployeesByIdsAsync(IEnumerable<string> ids)

        {
            

            var objectIds = ids.Select(id => ObjectId.Parse(id)).ToList();
            var filter = Builders<Employee>.Filter.In("_id", objectIds);
            return await _employees.Find(filter).ToListAsync();
        }

        //  Added by TAREK — Sorting functionality for Employees
        // This method allows sorting employees by any visible field.
        // Default: sort by Status ascending, then CreatedAt (if available).
        public async Task<List<Employee>> GetAllSortedAsync(string sortField = "Status", int sortOrder = 1)
        {
             Console.WriteLine($"[TAREK] Sorting Employees by {sortField} ({(sortOrder == 1 ? "ASC" : "DESC")})");
            var sortBuilder = Builders<Employee>.Sort;
            SortDefinition<Employee> sortDef;

            if (sortField == "Status")
            {
                //  Default sort (TAREK): first by Status, then by CreatedAt (if exists)
                sortDef = sortBuilder.Ascending(e => e.Status);

                // If model includes CreatedAt field, add it to the sorting
                try
                {
                    sortDef = sortDef.Ascending("CreatedAt");
                }
                catch {  }
            }
            else
            {
                //  Generic dynamic sort (TAREK): ASC or DESC
                sortDef = sortOrder == 1
                    ? sortBuilder.Ascending(sortField)
                    : sortBuilder.Descending(sortField);
            }

            return await _employees.Find(new BsonDocument())
                                   .Sort(sortDef)
                                   .ToListAsync();
        }
        //  END of TAREK’s sorting method
    }
}
