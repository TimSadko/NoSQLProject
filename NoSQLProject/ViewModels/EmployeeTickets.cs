using NoSQLProject.Models;

namespace NoSQLProject.ViewModels;

public class EmployeeTickets(List<Ticket> tickets, Employee employee)
{
    public List<Ticket> Tickets { get; set; } = tickets;
    public Employee Employee { get; set; } = employee;
}