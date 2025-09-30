using Microsoft.AspNetCore.Http;
using NoSQLProject.Models;

namespace NoSQLProject.Other
{
    public class Authorization
    {
        public static Employee? GetLoggedInEmployee(HttpContext context)
        {
            return context.Session.GetObject<Employee>("LoggedInEmployee");
        }
    }
}
