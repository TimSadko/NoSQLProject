namespace NoSQLProject.Models
{
    public class SDETicketEditViewModel
    {
        private Ticket _ticket;
        private List<Employee?> _log_employees;

        public SDETicketEditViewModel() { }

        public SDETicketEditViewModel(Ticket ticket, List<Employee?> log_employees)
        {
            _ticket = ticket;
            _log_employees = log_employees;
        }

        public Ticket Ticket { get => _ticket; set => _ticket = value; }
        public List<Employee?> LogEmployees {  get => _log_employees; set => _log_employees = value; }
    }
}
