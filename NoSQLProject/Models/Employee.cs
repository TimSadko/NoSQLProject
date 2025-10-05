using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace NoSQLProject.Models
{
    public enum Employee_Status
    {
        Active = 0, Deactivated = 1, Archived = 2
    }

	//[BsonDiscriminator(RootClass = true)]
	[BsonKnownTypes(typeof(ServiceDeskEmployee))]
	public class Employee
    {
        protected string _id = "";
		protected string _first_name = "";
		protected string _last_name = "";
		protected string _email = "";
		protected string _password = "";
		protected Employee_Status _status = 0;

        public Employee() { }

        public Employee(string id, string first_name, string last_name, string email, string password, Employee_Status status)
        {
            _id = id;
            _first_name = first_name;
            _last_name = last_name;
            _email = email;
            _password = password;
            _status = status;
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get => _id; set => _id = value; }

        [BsonElement("first_name")]
        [JsonPropertyName("first_name")]
        public string FirstName { get => _first_name; set => _first_name = value; }

        [BsonElement("last_name")]
        [JsonPropertyName("last_name")]
        public string LastName { get => _last_name; set => _last_name = value; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string Email { get => _email; set => _email = value; }

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public string Password { get => _password; set => _password = value; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public Employee_Status Status { get => _status; set => _status = value; }

        [BsonIgnore]
        [JsonIgnore]
        public string FullName { get => $"{_first_name} {_last_name}"; }
    }
}
