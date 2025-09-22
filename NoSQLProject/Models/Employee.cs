using MongoDB.Bson.Serialization.Attributes;

namespace NoSQLProject.Models
{
    public enum Employee_Roles
    {
        Normal = 0, Service_Desk = 1
    }

    public enum Employee_Status
    {
        Active = 0, Deactivated = 1, Archived = 2
    }

    public class Employee
    {
        private string? _id = "";
        private string _first_name = "";
        private string _last_name = "";
        private string _email = "";
        private string _password = "";
        private Employee_Roles _role = 0;
        private Employee_Status _status = 0;
        private List<Employee>? _managed_employees = null;

        public Employee() { }

        public Employee(string id, string first_name, string last_name, string email, string password, Employee_Roles role, Employee_Status status, List<Employee>? managed_employees)
        {
            _id = id;
            _first_name = first_name;
            _last_name = last_name;
            _email = email;
            _password = password;
            _role = role;
            _status = status;
            _managed_employees = managed_employees;
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get => _id; set => _id = value; }

        [BsonElement("first_name")]
        public string FirstName { get => _first_name; set => _first_name = value; }

        [BsonElement("last_name")]
        public string LastName { get => _last_name; set => _last_name = value; }

        [BsonElement("email")]
        public string Email { get => _email; set => _email = value; }

        [BsonElement("password")]
        public string Password { get => _password; set => _password = value; }

        [BsonElement("role")]
        public Employee_Roles Role { get => _role; set => _role = value; }

        [BsonElement("status")]
        public Employee_Status Status { get => _status; set => _status = value; }

        [BsonElement("managed_employees")]
        public List<Employee>? ManagedEmployees { get => _managed_employees; }
    }
}
