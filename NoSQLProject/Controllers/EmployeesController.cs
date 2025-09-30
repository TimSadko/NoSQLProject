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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _rep.GetAllAsync();

                return View(list);
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View();
            }
        }
    }
}
