namespace NoSQLProject.Models
{
	public class Log
	{
		private string _creator_id;
		private DateTime _created;
		private string _description;
		private Ticket_Status _new_status;

		public Log() { }

		public Log(string creator_id, DateTime created, string description, Ticket_Status new_status)
		{
			_creator_id = creator_id;
			_created = created;
			_description = description;
			_new_status = new_status;
		}

		public string Creator_id { get => _creator_id; set => _creator_id = value; }
		public DateTime Created { get => _created; set => _created = value; }
		public string Description { get => _description; set => _description = value; }
		public Ticket_Status NewStatus { get => _new_status; set => _new_status = value; }
	}
}
