using NoSQLProject.Models;

namespace NoSQLProject.Services
{
    /// <summary>
    /// Helper service for managing ticket priority operations
    /// </summary>
    public static class PriorityHelper
    {
        /// <summary>
        /// Gets the CSS class for priority badge styling
        /// </summary>
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

        /// <summary>
        /// Gets a human-readable display name for priority
        /// </summary>
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

        /// <summary>
        /// Sorts tickets by priority (descending) then by date (descending)
        /// </summary>
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

        /// <summary>
        /// Gets the numeric value for database sorting
        /// </summary>
        public static int GetPriorityValue(Ticket_Priority priority)
        {
            return (int)priority;
        }

        /// <summary>
        /// Ensures existing tickets without priority get default Undefined priority
        /// </summary>
        public static Ticket_Priority NormalizePriority(Ticket_Priority? priority)
        {
            return priority ?? Ticket_Priority.Undefined;
        }
    }
}
