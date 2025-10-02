using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace NoSQLProject.Models
{
    public class ServiceDeskEmployee : Employee
    {
		protected List<string>? _managed_employees_id = null;

		public ServiceDeskEmployee() { }

		public ServiceDeskEmployee(string id, string first_name, string last_name, string email, string password, Employee_Status status, List<string>? managed_employees_id) : base(id, first_name, last_name, email, password, status)
		{
			_managed_employees_id = managed_employees_id;
		}

		[BsonElement("managed_employees")]
		[JsonPropertyName("managed_employees")]
		public List<string>? ManagedEmployeesId { get => _managed_employees_id; set => _managed_employees_id = value; }
	}
}
