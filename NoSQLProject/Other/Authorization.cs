using Microsoft.AspNetCore.Http;
using NoSQLProject.Models;

namespace NoSQLProject.Other
{
    public class Authorization
    {
        public static Employee? GetLoggedInEmployee(HttpContext context)
        {
            var type = context.Session.GetString("LoggedInEmployeeType");

            if (type == null) return null;

            if(type == "s") return context.Session.GetObject<ServiceDeskEmployee>("LoggedInEmployee");
            else return context.Session.GetObject<Employee>("LoggedInEmployee");
        }

        public static void SetLoggedInEmployee(HttpContext context, Employee emp)
        {
            if (emp is ServiceDeskEmployee sde)
            {
                context.Session.SetString("LoggedInEmployeeType", "s");
                context.Session.SetObject("LoggedInEmployee", sde);
            }
            else 
            { 
                context.Session.SetString("LoggedInEmployeeType", "n");
                context.Session.SetObject("LoggedInEmployee", emp);
            }          
        }

        public static void RemoveLoggedInEmployee(HttpContext context)
        {
            context.Session.Clear();
        }
    }
}
