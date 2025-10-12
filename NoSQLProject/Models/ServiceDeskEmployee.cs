using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NoSQLProject.Models
{
    public class ServiceDeskEmployee : Employee
    {
        [BsonElement("managed_employees")]
        public List<string> ManagedEmployees { get; set; } = new();

        protected List<string>? _managed_employees_id = null;

		public ServiceDeskEmployee() { }

		public ServiceDeskEmployee(string id, string first_name, string last_name, string email, string password, Employee_Status status, List<string>? managed_employees_id) : base(id, first_name, last_name, email, password, status)
		{
			_managed_employees_id = managed_employees_id;
		}
    }
}

