namespace NoSQLProject.Models
{
    public class AddTicketRequestViewModel
    {
        private string _message;
        private string _email;
        private string _ticket_id;

        public AddTicketRequestViewModel() { }

        public AddTicketRequestViewModel(string message, string email, string ticket_id)
        {
            _message = message;
            _email = email;
            _ticket_id = ticket_id;
        }

        public string Message { get => _message; set => _message = value; }
        public string Email { get => _email; set => _email = value; }
        public string TicketId { get => _ticket_id; set => _ticket_id = value; }
    }
}
