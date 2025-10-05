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

                view_model.Employees = new List<Employee>(); // Create new list of employees

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
                return View();
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

                return View(t);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }           
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ticket t)
        {
            if (!Authenticate()) return RedirectToAction("Login", "Home");

            try
            {
                await _rep.CheckUpdateAsync(t);

                return View(t);
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

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);

            if (emp == null || emp is not ServiceDeskEmployee) return false;

            return true;
        }
    }
}
