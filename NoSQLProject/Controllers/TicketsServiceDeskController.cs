using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;

namespace NoSQLProject.Controllers
{
    public class TicketsServiceDeskController : Controller
    {
        private readonly ITicketRepository _rep;

        public TicketsServiceDeskController(ITicketRepository rep)
        {
            _rep = rep;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!Authenticate()) RedirectToAction("Login", "Home"); // If user is not logged in or not the right type, redirect to login page

            return View(await _rep.GetAllAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return View(new Ticket());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket t)
        {
            try
            {
                var emp = Authorization.GetLoggedInEmployee(HttpContext);

                if (emp == null) // If session is expired, redirect to loggin page
                {
                    TempData["Exception"] = "Your session is over, login and try again";
                    return RedirectToAction("Login", "Home");
                }
                
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

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);

            if (emp == null || emp is not ServiceDeskEmployee) return false;

            return true;
        }
    }
}
