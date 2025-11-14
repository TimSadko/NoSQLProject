using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using NoSQLProject.Models;
using NoSQLProject.Repositories;
using System.Net.Sockets;

namespace NoSQLProject.Services
{
	public class ServiceDeskEmployeeService : IServiceDeskEmployeeService
	{
		private readonly ITicketRepository _rep;
		private readonly IEmployeeRepository _employees_rep;
		private readonly ITicketRequestRepository _request_rep;

		public ServiceDeskEmployeeService(ITicketRepository rep, IEmployeeRepository employees_rep, ITicketRequestRepository request_rep)
		{
			_rep = rep;
			_employees_rep = employees_rep;
			_request_rep = request_rep;
		}

		public async Task<List<Ticket>> GetTicketsSortedAsync(string sortField, int sortOrder, bool archived = false)
		{
			List<Ticket> tickets;

			if (sortField == "CreatedBy")
			{
				tickets = await _rep.GetAllAsync(archived);
			}
			else if (sortField == "LogsNumber")
			{
				tickets = await _rep.GetAllAsync(archived);
				tickets.Sort((Ticket t, Ticket t2) => { return t.Logs.Count.CompareTo(t2.Logs.Count) * sortOrder; });
			}
			else
			{
				tickets = await _rep.GetAllSortedAsync(sortField, sortOrder, archived);
			}

			List<Task<Employee?>> tasks = new List<Task<Employee?>>();

			for (int i = 0; i < tickets.Count; i++)
			{
				tasks.Add(_employees_rep.GetByIdAsync(tickets[i].CreatedById));
			}

			await Task.WhenAll(tasks);

			for (int i = 0; i < tasks.Count; i++)
			{
				tickets[i].Creator = tasks[i].Result;
			}

			if (sortField == "CreatedBy")
			{
				tickets.Sort((Ticket t, Ticket t2) =>
				{
					if (t.Creator == null)
					{
						if (t2.Creator == null) return 0;
						else return sortOrder;
					}
					else
					{
						if (t2.Creator == null) return -sortOrder;
						else return t.Creator.FullName.CompareTo(t2.Creator.FullName) * sortOrder;
					}
				});
			}

			return tickets;
		}

		public async Task AddTicketAsync(Ticket ticket, Employee creator)
		{
			ticket.CreatedById = creator.Id;
			ticket.Status = Ticket_Status.Open;
			ticket.Logs = new List<Log>();
			ticket.CreatedAt = DateTime.UtcNow;
			ticket.UpdatedAt = DateTime.UtcNow;

			await _rep.AddAsync(ticket);
		}

		public async Task<Ticket> LoadTicketByIdAsync(string? id)
		{
			if (id == null) throw new ArgumentNullException("Id is null");

			Ticket? ticket = await _rep.GetByIdAsync((string)id);

			if (ticket == null) throw new ArgumentNullException($"Ticket with the id does not exist");

			List<Task<Employee?>> log_creator_tasks = new List<Task<Employee?>>();

			for (int i = 0; i < ticket.Logs.Count; i++)
			{
				log_creator_tasks.Add(_employees_rep.GetByIdAsync(ticket.Logs[i].CreatedById));
			}

			await Task.WhenAll(log_creator_tasks);

			for (int i = 0; i < log_creator_tasks.Count; i++)
			{
				ticket.Logs[i].Creator = log_creator_tasks[i].Result;
			}

			ticket.Creator = await _employees_rep.GetByIdAsync(ticket.CreatedById);

			return ticket;
		}

		public async Task<Ticket> GetTicketByIdAsync(string id)
		{
			Ticket? ticket = await _rep.GetByIdAsync(id);

			if (ticket == null) throw new Exception("Ticket with the id does not exist");

			return ticket;
		} 

		public async Task EditTicketAsync(Ticket ticket_new)
		{
			Ticket? ticket_old = await _rep.GetByIdAsync(ticket_new.Id);

			if (ticket_old == null) throw new Exception("Ticket with the id does not exist");

			if (ticket_old.Description != ticket_new.Description || ticket_old.Title != ticket_new.Title || ticket_old.Priority != ticket_new.Priority)
			{
				ticket_old.Description = ticket_new.Description;
				ticket_old.Title = ticket_new.Title;
				ticket_old.Priority = ticket_new.Priority;
				ticket_old.UpdatedAt = DateTime.UtcNow;

				await _rep.EditAsync(ticket_old);
			}
		}

		public async Task AddLogAsync(Ticket ticket, Log log, Employee creator)
		{
			List<Task> update_list = new List<Task>();

			log.Id = ObjectId.GenerateNewId().ToString();
			log.CreatedAt = DateTime.UtcNow;
			log.CreatedById = creator.Id;			

			update_list.Add(_rep.AddLogAsync(ticket, log, creator));
			update_list.Add(_rep.UpdateTicketStatusAsync(ticket.Id, log.NewStatus));

			var request_list = await _request_rep.GetRequestsByTicketAsync(ticket.Id);

			foreach (var r in request_list) // Go through the ticket requests, changing their status if needed 
			{
				if (r.Status == TicketRequestStatus.Open || r.Status == TicketRequestStatus.Accepted)
				{
					if (r.RecipientId == creator.Id)
					{
						if (log.NewStatus == Ticket_Status.Closed || log.NewStatus == Ticket_Status.Resolved)
						{
							update_list.Add(_request_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Fulfilled));
						}
					}
					else if (log.NewStatus != Ticket_Status.Open)
					{
						update_list.Add(_request_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Cancelled));
					}
				}
			}

			await Task.WhenAll(update_list);
		}

		public async Task<LogViewModel> GetLogViewModelAsync(string ticket_id, string log_id)
		{
			Ticket? t = await _rep.GetByIdAsync(ticket_id);

			if (t == null) throw new Exception($"Ticket with the id does not exist");

			Log? l = t.Logs.FirstOrDefault(log => log.Id == log_id);

			if (l == null) throw new Exception($"Log with the id does not exist");

			l.Creator = await _employees_rep.GetByIdAsync(l.CreatedById);

			return new LogViewModel(t, l);
		}

		public async Task EditLogAsync(string ticket_id, Log log)
		{
			await _rep.EditLogAsync(ticket_id, log);
		}

		public async Task DeleteTicketAsync(string ticket_id)
		{
			await _rep.DeleteAsync(ticket_id);
		}

		public async Task SetArchiveTicketAsync(string ticket_id, bool archive = true)
		{
			await _rep.SetArchiveAsync(ticket_id, archive);
		}

		public async Task DeleteLogAsync(string ticket_id, string log_id)
		{
			await _rep.DeleteLogAsync(ticket_id, log_id);
		}

		public async Task<string> UpdateStatusAsync(string ticket_id, string action_type, string logged_in_employee_id)
		{
			var ticket = await _rep.GetByIdAsync(ticket_id);

			if (ticket == null) throw new Exception("Ticket not found.");

			if (action_type == "escalate")
			{
				ticket.Status = Ticket_Status.Escalated;

				await _rep.EditAsync(ticket);

				await EscalateTicketsRequestsAsync(ticket_id);

				var managers = await _employees_rep.GetAllAsync();

				var manager = managers.FirstOrDefault(e => e is ServiceDeskEmployee);

				if (manager != null)
				{
					var request = new TicketRequest
					{
						TicketId = ticket.Id,
						SenderId = logged_in_employee_id,
						RecipientId = manager.Id,
						Message = $"Ticket '{ticket.Title}' has been escalated for review.",
						Status = TicketRequestStatus.Open,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
					};

					await _request_rep.AddAsync(request);
				}			

				return "Ticket escalated successfully and request sent to management.";
			}
			else if (action_type == "close")
			{
				ticket.Status = Ticket_Status.Closed;

				await _rep.EditAsync(ticket);

				await CloseTicketsRequestsAsync(ticket_id, logged_in_employee_id);

				return "Ticket closed successfully.";
			}
			else
			{
				throw new Exception("Unknown action type.");
			}
		}

		private async Task CloseTicketsRequestsAsync(string ticket_id, string logged_in_employee_id)
		{
			var requests = await _request_rep.GetRequestsByTicketAsync(ticket_id);

			List<Task> tasks = new List<Task>();

			foreach (var r in requests)
			{
				if (r.Status == TicketRequestStatus.Open || r.Status == TicketRequestStatus.Accepted)
				{
					if (r.RecipientId == logged_in_employee_id) tasks.Add(_request_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Fulfilled));				
					else tasks.Add(_request_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Cancelled));
				}
			}

			await Task.WhenAll(tasks);
		}

		private async Task EscalateTicketsRequestsAsync(string ticket_id)
		{
			var requests = await _request_rep.GetRequestsByTicketAsync(ticket_id);

			List<Task> tasks = new List<Task>();

			foreach (var r in requests)
			{
				if (r.Status == TicketRequestStatus.Open || r.Status == TicketRequestStatus.Accepted)
				{
					tasks.Add(_request_rep.UpdateRequestStatusAsync(r.Id, TicketRequestStatus.Redirected));
				}
			}

			await Task.WhenAll(tasks);
		}
	}
}
