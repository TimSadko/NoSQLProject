using System;

namespace NoSQLProject.Models
{
    public enum Ticket_Status // Do not know what to put here, if you work with tickets fill this in!
    {

    }

    public class Ticket
    {
        private string _id = "";
        private string _created_by_id = "";
        private string _assigned_to_id = "";
        private string _title = "";
        private string _description = "";
        private Ticket_Status _status = 0;
        private DateTime _created_at;
        private DateTime _updated_at;

        public Ticket() { }

        public Ticket(string id, string created_by_id, string assigned_to_id, string title, string description, Ticket_Status status, DateTime created_at, DateTime updated_at)
        {
            _id = id;
            _created_by_id = created_by_id;
            _assigned_to_id = assigned_to_id;
            _title = title;
            _description = description;
            _status = status;
            _created_at = created_at;
            _updated_at = updated_at;
        }

        public string Id { get => _id; set => _id = value; }
        public string CreatedById { get => _created_by_id; set => _created_by_id = value; }
        public string AssignedToId { get => _assigned_to_id; set => _assigned_to_id = value; }
        public string Title { get => _title; set => _title = value; }
        public string Description { get => _description; set => _description = value; }
        public Ticket_Status Status { get => _status; set => _status = value; }
        public DateTime CreatedAt { get => _created_at; set => _created_at = value; }
        public DateTime UpdatedAt { get => _updated_at; set => _updated_at = value; }
    }
}
