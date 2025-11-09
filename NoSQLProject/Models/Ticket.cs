using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace NoSQLProject.Models
{
    public enum Ticket_Status
    {
        Open = 0, Closed = 1, Resolved = 2
    }

    public enum Ticket_Priority
    {
        Undefined = 0, 
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public class Ticket
    {
        private string _id = "";
        private string _created_by_id = "";
        private List<Log> _logs = new List<Log>();
        private string _title = "";
        private string _description = "";
        private Ticket_Status _status = 0;
        private Ticket_Priority _priority = Ticket_Priority.Undefined;
        private DateTime _created_at;
        private DateTime _updated_at;

        private Employee? _creator = null;

        public Ticket() { }

        public Ticket(string id, string created_by_id, List<Log> logs, string title, string description, Ticket_Status status, Ticket_Priority priority, DateTime created_at, DateTime updated_at)
        {
            _id = id;
            _created_by_id = created_by_id;
            _logs = logs;
            _title = title;
            _description = description;
            _status = status;
            _priority = priority;
            _created_at = created_at;
            _updated_at = updated_at;
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get => _id; set => _id = value; }

        [BsonElement("created_by")]
        [JsonPropertyName("created_by")]
        public string CreatedById { get => _created_by_id; set => _created_by_id = value; }

        [BsonElement("logs")]
        [JsonPropertyName("logs")]
        public List<Log> Logs { get => _logs; set => _logs = value; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string Title { get => _title; set => _title = value; }

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get => _description; set => _description = value; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public Ticket_Status Status { get => _status; set => _status = value; }

        [BsonElement("priority")]
        [JsonPropertyName("priority")]
        public Ticket_Priority Priority { get => _priority; set => _priority = value; }

        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get => _created_at; set => _created_at = value; }

        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get => _updated_at; set => _updated_at = value; }

        [BsonIgnore]
        [JsonIgnore]
        public Employee? Creator { get => _creator; set => _creator = value; }

        public override string? ToString()
        {
            return $"Ticket: {{\nid: {_id},\ncreated_by_id: {_created_by_id},\ncreated_at: {_created_at},\nupdated_at: {_updated_at},\ntitle: {_title},\ndesc: {_description},\nstatus: {_status},\npriority: {_priority},\nlogs: {_logs.Count}\n}}";
        }
    }
}