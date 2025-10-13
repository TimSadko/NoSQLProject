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
        public async Task<IActionResult> Index()
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home"); // If user is not logged in or not the right type, redirect to login page

            try
            {
                var view_model = new SDETickestsListViewModel(); // Create new view model

                view_model.Tickets = await _rep.GetAllAsync(); // Get all ticket

                view_model.Employees = new List<Employee?>(); // Create new list of employees

                List<Task<Employee?>> tasks = new List<Task<Employee?>>(); // Create new list of tasks, in order to read all of the employees in parallel

                for (int i = 0; i < view_model.Tickets.Count; i++)
                {
                    tasks.Add(_employees_rep.GetByIdAsync(view_model.Tickets[i].CreatedById));
                }

                await Task.WhenAll(tasks); // Wait for all of the employees to load

                for (int i = 0; i < tasks.Count; i++) // Add all of loaded the employees to the view model
                {
                    view_model.Employees.Add(tasks[i].Result); 
                }

                return View(view_model);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(new SDETickestsListViewModel(new List<Ticket>(), new List<Employee?>()));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Add()
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

                t.CreatedAt = DateTime.Now;
                t.UpdatedAt = DateTime.Now;

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

                Ticket? t = await _rep.GetByIdAsync((string)id);

                if (t == null) throw new ArgumentNullException($"Ticket with Id({id}) does not exist");

                var view_model = new SDETicketEditViewModel(t, new List<Employee?>());

                List<Task<Employee?>> tasks = new List<Task<Employee?>>(); // Create new list of tasks, in order to read all of the employees in parallel

                for (int i = 0; i < view_model.Ticket.Logs.Count; i++)
                {
                    tasks.Add(_employees_rep.GetByIdAsync(t.Logs[i].CreatedById)); 
                }

                await Task.WhenAll(tasks); // Wait for all of the employees to load

                for (int i = 0; i < tasks.Count; i++) // Add all of loaded the employees to the view model
                {
                    view_model.LogEmployees.Add(tasks[i].Result);
                }

                Employee? creator = await _employees_rep.GetByIdAsync(t.CreatedById);

                ViewData["creator"] = creator == null ? "???" : creator.FullName; 

                return View(view_model);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }           
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SDETicketEditViewModel view_model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _rep.CheckUpdateAsync(view_model.Ticket);

                return RedirectToAction("Edit", new {id = view_model.Ticket.Id});
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

                Ticket? t = await _rep.GetByIdAsync((string)id);

                if (t == null) throw new ArgumentNullException($"Ticket with Id({id}) does not exist");

                var view_model = new SDETicketEditViewModel(t, new List<Employee?>());

                List<Task<Employee?>> tasks = new List<Task<Employee?>>(); // Create new list of tasks, in order to read all of the employees in parallel

                for (int i = 0; i < view_model.Ticket.Logs.Count; i++)
                {
                    tasks.Add(_employees_rep.GetByIdAsync(t.Logs[i].CreatedById));
                }

                await Task.WhenAll(tasks); // Wait for all of the employees to load

                for (int i = 0; i < tasks.Count; i++) // Add all of loaded the employees to the view model
                {
                    view_model.LogEmployees.Add(tasks[i].Result);
                }

                Employee? creator = await _employees_rep.GetByIdAsync(t.CreatedById);

                ViewData["creator"] = creator == null ? "???" : creator.FullName;

                return View(view_model);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(SDETicketEditViewModel view_model)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                Console.WriteLine(view_model.Ticket.Id);
                await _rep.DeleteAsync(view_model.Ticket.Id);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);

            if (emp == null || emp is not ServiceDeskEmployee) return false;

            return true;
        }
    }
}
