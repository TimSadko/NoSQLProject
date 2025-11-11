using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Repositories;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace NoSQLProject.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = Other.Security.BasicAuthenticationHandler.SchemeName)]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            this.ticketRepository = ticketRepository;
        }

        // GET: api/tickets
        // Optional filters: employeeId, sortField, sortOrder (1 ascending, -1 descending)
        [HttpGet]
        public async Task<ActionResult<List<Ticket>>> GetAll([FromQuery] string? employeeId,
            [FromQuery] string sortField = "CreatedAt", [FromQuery] int sortOrder = -1)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isServiceDesk = User.IsInRole("ServiceDesk");

            // Sanitize sort options to avoid runtime errors and keep predictable behavior
            (string safeSortField, int safeSortOrder) = SanitizeSortOptions(sortField, sortOrder);

            List<Ticket> result;

            if (isServiceDesk)
            {
                if (!string.IsNullOrWhiteSpace(employeeId))
                {
                    result = await ticketRepository.GetAllByEmployeeIdAsync(employeeId);
                }
                else
                {
                    result = await ticketRepository.GetAllSortedAsync(safeSortField, safeSortOrder);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(userId)) return Forbid();
                result = await ticketRepository.GetAllByEmployeeIdAsync(userId);
            }

            return Ok(result);
        }

        private static (string Field, int Order) SanitizeSortOptions(string sortField, int sortOrder)
        {
            // Allowlist of sortable fields mapped to Mongo property names
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CreatedAt", "UpdatedAt", "Title", "Status"
            };

            var field = allowed.Contains(sortField) ? sortField : "CreatedAt";
            var order = sortOrder == 1 ? 1 : -1;
            return (field, order);
        }

        // GET: api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetById(string id)
        {
            var ticket = await ticketRepository.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            var isServiceDesk = User.IsInRole("ServiceDesk");
            if (isServiceDesk)
            {
                return Ok(ticket);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) ||
                !string.Equals(ticket.CreatedById, userId, StringComparison.Ordinal)) return Forbid();

            return Ok(ticket);
        }

        // POST: api/tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> Create([FromBody] CreateTicketRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                CreatedById = request.CreatedById,
                Status = request.Status ?? Ticket_Status.Open,
                Logs = [],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await ticketRepository.AddAsync(ticket);

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTicketRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existing = await ticketRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Apply updates if provided
            if (request.Title != null) existing.Title = request.Title;
            if (request.Description != null) existing.Description = request.Description;
            if (request.Status.HasValue) existing.Status = request.Status.Value;

            existing.UpdatedAt = DateTime.UtcNow;

            // Use fine-grained update to avoid replacing the whole doc
            await ticketRepository.EditAsync(existing);

            return NoContent();
        }

        // DELETE: api/tickets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await ticketRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await ticketRepository.DeleteAsync(id);
            return NoContent();
        }

        // DTOs for requests
        public class CreateTicketRequest
        {
            [Required] public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            [Required] public string CreatedById { get; set; } = string.Empty;
            public Ticket_Status? Status { get; set; }
        }

        public class UpdateTicketRequest
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public Ticket_Status? Status { get; set; }
        }
    }
}