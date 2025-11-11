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

		public async Task<List<Ticket>> GetTicketsSortedAsync(string sortField, int sortOrder)
		{
			List<Ticket> tickets;

			if (sortField == "CreatedBy")
			{
				tickets = await _rep.GetAllAsync();
			}
			else if (sortField == "LogsNumber")
			{
				tickets = await _rep.GetAllAsync();
				tickets.Sort((Ticket t, Ticket t2) => { return t.Logs.Count.CompareTo(t2.Logs.Count) * sortOrder; });
			}
			else
			{
				tickets = await _rep.GetAllSortedAsync(sortField, sortOrder);
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

			Employee? ticket_creator = await _employees_rep.GetByIdAsync(ticket.CreatedById);

			ticket.Creator = ticket_creator;

			return ticket;
		}

		public async Task<Ticket> GetTicketByIdAsync(string id)
		{
			Ticket? ticket = await _rep.GetByIdAsync(id);

			if (ticket == null) throw new Exception("Ticket with the id does not exsist");

			return ticket;
		} 

		public async Task EditTicketAsync(Ticket ticket_new)
		{
			Ticket? ticket_old = await _rep.GetByIdAsync(ticket_new.Id);

			if (ticket_old == null) throw new Exception("Ticket with the id does not exsist");

			if (ticket_old.Description != ticket_new.Description ||
				ticket_old.Title != ticket_new.Title ||
				ticket_old.Status != ticket_new.Status ||
				ticket_old.Priority != ticket_new.Priority)
			{
				ticket_old.Description = ticket_new.Description;
				ticket_old.Title = ticket_new.Title;
				ticket_old.Status = ticket_new.Status;
				ticket_old.Priority = ticket_new.Priority;
				ticket_old.UpdatedAt = DateTime.UtcNow;

				await _rep.EditAsync(ticket_old);
			}
		}

		public async Task AddLogAsync(Ticket ticket, Log log, Employee creator)
		{
			log.Id = ObjectId.GenerateNewId().ToString();
			log.CreatedAt = DateTime.UtcNow;
			log.CreatedById = creator.Id;

			await _rep.AddLogAsync(ticket, log, creator);
		}

		public async Task<LogViewModel> GetLogViewModelAsync(string ticket_id, string log_id)
		{
			Ticket? t = await _rep.GetByIdAsync(ticket_id);

			if (t == null) throw new Exception($"Ticket with Id({ticket_id}) does not exist");

			Log? l = t.Logs.FirstOrDefault(log => log.Id == log_id);

			if (l == null) throw new Exception($"Log with Id({log_id}) does not exist");

			Employee? creator = await _employees_rep.GetByIdAsync(l.CreatedById);

			l.Creator = creator;

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

		public async Task DeleteLogAsync(string ticket_id, string log_id)
		{
			await _rep.DeleteLogAsync(ticket_id, log_id);
		}
	}
}
