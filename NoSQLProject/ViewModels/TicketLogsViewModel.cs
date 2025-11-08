using NoSQLProject.Models;

namespace NoSQLProject.ViewModels;

public class TicketLogsViewModel(List<Tuple<Log, Employee>> employeeLogPairs)
{
    public List<Tuple<Log, Employee>> EmployeeLogPairs { get; set; } = employeeLogPairs;
}