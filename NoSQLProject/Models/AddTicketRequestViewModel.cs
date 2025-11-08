namespace NoSQLProject.Models
{
    public class AddTicketRequestViewModel
    {
        private TicketRequest _request;
        private string _email;

        public AddTicketRequestViewModel() { }

        public AddTicketRequestViewModel(TicketRequest request, string email)
        {
            _request = request;
            _email = email;
        }

        public TicketRequest Request { get => _request; set => _request = value; }
        public string Email { get => _email; set => _email = value; }
    }
}
