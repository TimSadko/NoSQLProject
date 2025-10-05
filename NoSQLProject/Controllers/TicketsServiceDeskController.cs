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
            if (!Authenticate()) RedirectToAction("Login", "Home");

            return View(new Ticket());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket t)
        {
            if (!Authenticate()) RedirectToAction("Login", "Home");

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
            if (!Authenticate()) RedirectToAction("Login", "Home");

            try
            {
                if(id == null) throw new ArgumentNullException("Id is null");

                Ticket t = await _rep.GetByIdAsync((string)id);

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
            if (!Authenticate()) RedirectToAction("Login", "Home");

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

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);

            if (emp == null || emp is not ServiceDeskEmployee) return false;

            return true;
        }
    }
}
