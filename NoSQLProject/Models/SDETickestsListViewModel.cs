namespace NoSQLProject.Models
{
    public class SDETickestsListViewModel
    {
        private List<Ticket> _tickets;
        private List<Employee?> _employees;

        public SDETickestsListViewModel() { }

        public SDETickestsListViewModel(List<Ticket> tickets, List<Employee?> employees)
        {
            _tickets = tickets;
            _employees = employees;
        }

        public List<Ticket> Tickets { get => _tickets; set => _tickets = value; }
        public List<Employee?> Employees { get => _employees; set => _employees = value; }
    }
}
