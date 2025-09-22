using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Repositories;

namespace NoSQLProject.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _rep;

        public EmployeesController(IEmployeeRepository rep)
        {
            _rep = rep;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _rep.GetAllAsync();

            return View(list);
        }
    }
}
