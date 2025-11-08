using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace NoSQLProject.Models
{
    public enum TicketRequestStatus
    {
        Open = 0, Accepted = 1, Rejected = 2, Fulfilled = 3, Redirected = 4, Failed = 5
    }

    public class TicketRequest
    {
        private string _id;
        private string _sender_id;
        private string _recipient_id;
        private string _ticket_id;
        private string _message;
        private DateTime _created_at;
        private DateTime _updated_at;
        private TicketRequestStatus _status;
        private bool _archived = false;

        private Employee? _sender = null;
        private Employee? _recipient = null;
        private Ticket? _ticket = null;

        public TicketRequest() { }

        public TicketRequest(string id, string sender_id, string recipient_id, string ticket_id, string message, DateTime created_at, DateTime updated_at, TicketRequestStatus status, bool archived)
        {
            _id = id;
            _sender_id = sender_id;
            _recipient_id = recipient_id;
            _ticket_id = ticket_id;
            _message = message;
            _created_at = created_at;
            _updated_at = updated_at;
            _status = status;
            _archived = archived;
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get => _id; set => _id = value; }

        [BsonElement("sender_id")]
        [JsonPropertyName("sender_id")]
        public string SenderId { get => _sender_id; set => _sender_id = value; }

        [BsonElement("recipient_id")]
        [JsonPropertyName("recipient_id")]
        public string RecipientId { get => _recipient_id; set => _recipient_id = value; }

        [BsonElement("ticket_id")]
        [JsonPropertyName("ticket_id")]
        public string TicketId { get => _ticket_id; set => _ticket_id = value; }

        [BsonElement("message")]
        [JsonPropertyName("message")]
        public string Message { get => _message; set => _message = value; }

        [BsonElement("created_at")]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get => _created_at; set => _created_at = value; }

        [BsonElement("updated_at")]
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get => _updated_at; set => _updated_at = value; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public TicketRequestStatus Status { get => _status; set => _status = value; }

        [BsonElement("archived")]
        [JsonPropertyName("archived")]
        public bool Archived { get => _archived; set => _archived = value; }


        [BsonIgnore]
        [JsonIgnore]
        public Employee? Sender { get => _sender; set => _sender = value; }

        [BsonIgnore]
        [JsonIgnore]
        public Employee? Recipient { get => _recipient; set => _recipient = value; }

        [BsonIgnore]
        [JsonIgnore]
        public Ticket? Ticket { get => _ticket; set => _ticket = value; }

    }
}
