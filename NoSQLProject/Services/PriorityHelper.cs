using NoSQLProject.Models;

namespace NoSQLProject.Services
{
    public static class PriorityHelper
    {
        public static string GetPriorityBadgeClass(Ticket_Priority priority)
        {
            return priority switch
            {
                Ticket_Priority.Critical => "priority-critical",
                Ticket_Priority.High => "priority-high",
                Ticket_Priority.Medium => "priority-medium",
                Ticket_Priority.Low => "priority-low",
                Ticket_Priority.Undefined => "priority-undefined",
                _ => "priority-undefined"
            };
        }

        public static string GetPriorityDisplayName(Ticket_Priority priority)
        {
            return priority switch
            {
                Ticket_Priority.Critical => "🔴 Critical",
                Ticket_Priority.High => "🟠 High",
                Ticket_Priority.Medium => "🟡 Medium",
                Ticket_Priority.Low => "🟢 Low",
                Ticket_Priority.Undefined => "⚪ Undefined",
                _ => "⚪ Undefined"
            };
        }

        public static List<Ticket> SortByPriority(List<Ticket> tickets, bool ascending = false)
        {
            if (ascending)
            {
                return tickets.OrderBy(t => t.Priority)
                             .ThenByDescending(t => t.CreatedAt)
                             .ToList();
            }
            else
            {
                return tickets.OrderByDescending(t => t.Priority)
                             .ThenByDescending(t => t.CreatedAt)
                             .ToList();
            }
        }

        public static int GetPriorityValue(Ticket_Priority priority)
        {
            return (int)priority;
        }

        public static Ticket_Priority NormalizePriority(Ticket_Priority? priority)
        {
            return priority ?? Ticket_Priority.Undefined;
        }
    }
}
