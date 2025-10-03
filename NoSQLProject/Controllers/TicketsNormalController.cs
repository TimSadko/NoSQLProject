using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;

namespace NoSQLProject.Controllers
{
    public class TicketsNormalController : Controller
    {
        private readonly ITicketRepository _rep;

        public TicketsNormalController(ITicketRepository rep)
        {
            _rep = rep;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            throw new NotImplementedException();

            if (!Authenticate()) RedirectToAction("Login", "Home"); // If user is not logged in or not the right type, redirect to login page
            //Console.WriteLine((await _rep.GetByIdAsync("68dbb2dc167cf48344f69e0b")).Title);           

            return View();
        }

        public bool Authenticate()
        {
            Employee? emp = Authorization.GetLoggedInEmployee(this.HttpContext);

            if (emp == null || emp is ServiceDeskEmployee) return false;

            return true;
        }
    }
}
