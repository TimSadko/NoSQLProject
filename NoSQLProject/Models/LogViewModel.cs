namespace NoSQLProject.Models
{
    public class LogViewModel
    {
        private Ticket _ticket;
        private Log _log;

        public LogViewModel() { }

        public LogViewModel(Ticket ticket, Log log)
        {
            _ticket = ticket;
            _log = log;
        }

        public Ticket Ticket { get => _ticket; set => _ticket = value; }
        public Log Log { get => _log; set => _log = value; }
    }
}
