
using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;

namespace NoSQLProject.Controllers
{
    public class TicketsServiceDeskController : Controller
    {
        private readonly ITicketRepository _rep;
        private readonly IEmployeeRepository _employees_rep;

        public TicketsServiceDeskController(ITicketRepository rep, IEmployeeRepository employees_rep)
        {
            _rep = rep;
            _employees_rep = employees_rep;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortField = "CreatedAt", int sortOrder = -1)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home"); 

            try
            {
                List<Ticket> view_model;

                if(sortField == "CreatedBy")
                {
                    view_model = await _rep.GetAllAsync();
                }
                else if(sortField == "LogsNumber")
                {
                    view_model = await _rep.GetAllAsync();

                    if(sortOrder == 1) view_model.Sort((Ticket t, Ticket t2) => { return t.Logs.Count.CompareTo(t2.Logs.Count); });
                    else view_model.Sort((Ticket t, Ticket t2) => { return t2.Logs.Count.CompareTo(t.Logs.Count); });
                }
                else
                {
                    view_model = await _rep.GetAllSortedAsync(sortField, sortOrder);
                }              


                List<Task<Employee?>> tasks = new List<Task<Employee?>>();

                for (int i = 0; i < view_model.Count; i++)
                {
                    tasks.Add(_employees_rep.GetByIdAsync(view_model[i].CreatedById));
                }

                await Task.WhenAll(tasks);

                for (int i = 0; i < tasks.Count; i++) 
                {
                    view_model[i].Creator = tasks[i].Result; 
                }

                if (sortField == "CreatedBy")
                {
                    view_model.Sort((Ticket t, Ticket t2) =>
                    {
                        if (t.Creator == null)
                        {
                            if (t2.Creator == null) return 0;
                            else return sortOrder;
                        }
                        else
                        {
                            if(t2.Creator == null) return -sortOrder;
                            else return t.Creator.FullName.CompareTo(t2.Creator.FullName) * sortOrder;
                        }
                    });
                }

                return View(view_model);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new List<Ticket>());
            }
        }


        [HttpGet]
        public IActionResult Add()
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            return View(new Ticket());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket t)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                var emp = Authorization.GetLoggedInEmployee(HttpContext);

                t.CreatedById = emp.Id;
                t.Status = Ticket_Status.Open;
                t.Logs = new List<Log>();
                t.CreatedAt = DateTime.UtcNow;
                t.UpdatedAt = DateTime.UtcNow;

                await _rep.AddAsync(t);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(t);
            }
        }

        [HttpGet("TicketsServiceDesk/Edit/{id}")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                if(id == null) throw new ArgumentNullException("Id is null");

                Ticket? ticket = await _rep.GetByIdAsync((string)id);

                if (ticket == null) throw new ArgumentNullException($"Ticket with Id({id}) does not exist");

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

                ViewData["ticket_creator"] = ticket_creator == null ? "???" : ticket_creator.FullName; 

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ticket ticket)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _rep.CheckUpdateAsync(ticket);

                return RedirectToAction("Edit", new {id = ticket.Id});
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("TicketsServiceDesk/AddLog/{id}")]
        public async Task<IActionResult> AddLog(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                Ticket? t = await _rep.GetByIdAsync(id);

                if (t == null) throw new Exception($"Ticket with Id({id}) does not exist");

                Log l = new Log();

                l.NewStatus = t.Status;

                return View(new LogViewModel(t, l));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLog(LogViewModel model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                var emp = Authorization.GetLoggedInEmployee(this.HttpContext);

                if (emp == null) return RedirectToAction("Login", "Home");

                await _rep.AddLogAsync(model.Ticket, model.Log, emp);

                return RedirectToAction("Edit", new { id = model.Ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Edit", new { id = model.Ticket.Id });
            }
        }

        [HttpGet("TicketsServiceDesk/EditLog/{ticket_id}/{log_id}")]
        public async Task<IActionResult> EditLog(string ticket_id, string log_id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                Ticket? t = await _rep.GetByIdAsync(ticket_id);

                if (t == null) throw new Exception($"Ticket with Id({ticket_id}) does not exist");

                Log? l = t.Logs.FirstOrDefault(log => log.Id == log_id);

                if (l == null) throw new Exception($"Log with Id({log_id}) does not exist");

                Employee? creator = await _employees_rep.GetByIdAsync(l.CreatedById);

                ViewData["creator"] = creator == null ? "???" : creator.FullName;

                return View(new LogViewModel(t, l));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditLog(LogViewModel view_model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _rep.EditLogAsync(view_model.Ticket.Id, view_model.Log);

                return RedirectToAction("Edit", new { id = view_model.Ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                if (id == null) throw new ArgumentNullException("Id is null");

                Ticket? ticket = await _rep.GetByIdAsync((string)id);

                if (ticket == null) throw new ArgumentNullException($"Ticket with Id({id}) does not exist");

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

                Employee? creator = await _employees_rep.GetByIdAsync(ticket.CreatedById);

                ViewData["ticket_creator"] = creator == null ? "???" : creator.FullName;

                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Ticket ticket)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _rep.DeleteAsync(ticket.Id);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("TicketsServiceDesk/DeleteLog/{ticket_id}/{log_id}")]
        public async Task<IActionResult> DeleteLog(string ticket_id, string log_id) 
        { 
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                Ticket? t = await _rep.GetByIdAsync(ticket_id);

                if (t == null) throw new Exception($"Ticket with Id({ticket_id}) does not exist");

                Log? l = t.Logs.FirstOrDefault(log => log.Id == log_id);

                if (l == null) throw new Exception($"Log with Id({log_id}) does not exist");

                Employee? creator = await _employees_rep.GetByIdAsync(l.CreatedById);

                ViewData["creator"] = creator == null ? "???" : creator.FullName;

                return View(new LogViewModel(t, l));
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLog(LogViewModel view_model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _rep.DeleteLogAsync(view_model.Ticket.Id, view_model.Log.Id);

                return RedirectToAction("Edit", new { id = view_model.Ticket.Id });
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        private bool Authenticate()
        {
            var emp = Authorization.GetLoggedInEmployee(HttpContext);
            return emp is ServiceDeskEmployee;
        }
    }
}
