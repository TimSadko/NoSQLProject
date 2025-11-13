using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NoSQLProject.Repositories;

namespace NoSQLProject.Controllers
{
    public class LogsController : Controller
    {
        private readonly LogRepository _logRepository;

        public LogsController(IMongoDatabase db)
        {
            _logRepository = new LogRepository(db);
        }

        public async Task<IActionResult> Index(string sortField = "CreatedAt", int sortOrder = -1)
        {
            var logs = await _logRepository.GetAllSortedAsync(sortField, sortOrder);
            return View(logs);
        }
    }
}
