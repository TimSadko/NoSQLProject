using MongoDB.Bson;
using MongoDB.Driver;
using NoSQLProject.Models;

namespace NoSQLProject.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;
        private readonly IMongoCollection<TicketRequest> _requests;

        public EmployeeRepository(IMongoDatabase db)
        {
            _employees = db.GetCollection<Employee>("employees");
			_requests = db.GetCollection<TicketRequest>("ticket_requests");
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
            var filter = builder.And(builder.Eq("email", login), builder.Eq("password", password));
            return await _employees.FindAsync(filter).Result.FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Email, email);
            return await _employees.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            await _employees.InsertOneAsync(employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Id, employee.Id);
            await _employees.ReplaceOneAsync(filter, employee);
        }

        public async Task DeleteAsync(Employee employee)
        {
            List<Task<DeleteResult>> delete_tasks = new List<Task<DeleteResult>>();

            delete_tasks.Add(_requests.DeleteManyAsync(Builders<TicketRequest>.Filter.Or(Builders<TicketRequest>.Filter.Eq("sender_id", employee.Id), Builders<TicketRequest>.Filter.Eq("recipient_id", employee.Id))));

            delete_tasks.Add(_employees.DeleteOneAsync(Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(employee.Id))));

            await Task.WhenAll(delete_tasks);
        }

        public async Task<List<Employee>> GetEmployeesByIdsAsync(IEnumerable<string> ids)
        {
            var filter = Builders<Employee>.Filter.In(e => e.Id, ids);
            return await _employees.Find(filter).ToListAsync();
        }

        public async Task<List<Employee>> GetAllSortedAsync(string sortField = "Status", int sortOrder = 1)
        {
            var sortBuilder = Builders<Employee>.Sort;
            SortDefinition<Employee> sortDef;

            if (sortField == "Status")
            {             
                sortDef = sortBuilder.Ascending(e => e.Status);

                try
                {
                    sortDef = sortDef.Ascending("CreatedAt");
                }
                catch {  }
            }
            else
            {
                sortDef = sortOrder == 1 ? sortBuilder.Ascending(sortField) : sortBuilder.Descending(sortField);
            }

            return await _employees.Find(new BsonDocument()).Sort(sortDef).ToListAsync();
        }

        public async Task<List<Employee>> GetByStatusAggregationAsync(string status)
        {
            if (!Enum.TryParse<Employee_Status>(status, out var enumStatus)) return new List<Employee>();
          
            var aggregation = _employees.Aggregate().Match(Builders<Employee>.Filter.Eq(e => e.Status, enumStatus)); // Use an aggregation pipeline to match by status

            return await aggregation.ToListAsync();
        }

        public async Task<List<Employee>> GetByStatusAsync(string status)
        {
            if (!Enum.TryParse<Employee_Status>(status, out var enumStatus)) return new List<Employee>();

            var filter = Builders<Employee>.Filter.Eq(e => e.Status, enumStatus);

            return await _employees.Find(filter).ToListAsync();
        }

    }
}
