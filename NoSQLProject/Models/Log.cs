using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace NoSQLProject.Models
{
	public class Log
	{
		private string _id;
		private string _created_by_id;
		private DateTime _created_at;
		private string _description;
		private Ticket_Status _new_status;

		public Log() { }

		public Log(string id, string created_by_id, DateTime created_at, string description, Ticket_Status new_status)
		{
			_id = id;	
			_created_by_id = created_by_id;
			_created_at = created_at;
			_description = description;
			_new_status = new_status;
		}

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get => _id; set => _id = value; }

        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        public string CreatedById { get => _created_by_id; set => _created_by_id = value; }

        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get => _created_at; set => _created_at = value; }

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get => _description; set => _description = value; }

        [BsonElement("new_status")]
        [JsonPropertyName("new_status")]
        public Ticket_Status NewStatus { get => _new_status; set => _new_status = value; }
	}
}
