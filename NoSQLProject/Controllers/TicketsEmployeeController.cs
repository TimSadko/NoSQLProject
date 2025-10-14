﻿using Microsoft.AspNetCore.Mvc;
using NoSQLProject.Models;
using NoSQLProject.Other;
using NoSQLProject.Repositories;
using NoSQLProject.ViewModels;

namespace NoSQLProject.Controllers
{
    public class TicketsEmployeeController(
        ITicketRepository ticketRepository,
        IEmployeeRepository employeeRepository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedEmployee = Authenticate();
            if (authenticatedEmployee?.Id == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var tickets = await ticketRepository.GetAllByEmployeeIdAsync(authenticatedEmployee.Id);
            var employeeTickets = new EmployeeTickets(tickets, authenticatedEmployee);
            return View(employeeTickets);
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }

            return View(new Ticket());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Ticket ticket)
        {
            var authenticatedEmployee = Authenticate();

            if (authenticatedEmployee?.Id == null)
                return RedirectToAction("Login", "Home");

            try
            {
                ticket.CreatedById = authenticatedEmployee.Id;
                ticket.Status = Ticket_Status.Open;
                ticket.Logs = [];
                ticket.CreatedAt = DateTime.Now;
                ticket.UpdatedAt = DateTime.Now;
                await ticketRepository.AddAsync(ticket);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["Exception"] = ex.Message;
                return View(ticket);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }

            if (string.IsNullOrEmpty(id)) throw new Exception("Ticket id is empty or null!");

            var ticket = await ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                TempData["Exception"] = "Ticket is null. Something went wrong!";
            }

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Ticket? ticket)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                if (string.IsNullOrEmpty(ticket?.Id)) throw new Exception("Ticket id is empty or null!");

                var ticketToChange = await ticketRepository.GetByIdAsync(ticket.Id);
                if (ticketToChange == null)
                {
                    TempData["Exception"] = "Ticket can not be found. Something went wrong!";
                    return View(ticket);
                }

                ticketToChange.Title = ticket.Title;
                ticketToChange.Description = ticket.Description;
                await ticketRepository.EditAsync(ticketToChange);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }

            if (string.IsNullOrEmpty(id)) throw new Exception("Ticket id is empty or null!");

            var ticket = await ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                TempData["Exception"] = "Ticket is null. Something went wrong!";
            }

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Ticket? ticket)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                if (string.IsNullOrEmpty(ticket?.Id)) throw new Exception("Ticket id is empty or null!");
               
                var ticketToRemove = await ticketRepository.GetByIdAsync(ticket.Id);
                if (ticketToRemove == null)
                {
                    TempData["Exception"] = "Ticket can not be found. Something went wrong!";
                    return View(ticket);
                }

                if (ticketToRemove.Logs.Count > 0)
                {
                    TempData["Exception"] = "Ticket has some logs. You can not delete it!";
                    return View(ticket);
                }
                
                await ticketRepository.DeleteAsync(ticket.Id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("TicketsEmployee/Logs/{id}")]
        public async Task<IActionResult> Logs(string? id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Home");

            try
            {
                if (string.IsNullOrEmpty(id)) throw new Exception("Ticket id is empty or null!");
                var logs = await ticketRepository.GetLogsByTicketIdAsync(id);

                if (logs.Count == 0)
                {
                    TempData["Exception"] = "No logs found for this ticket.";
                    return RedirectToAction("Index");
                }

                List<Tuple<Log, Employee>> employeeLogPairs = [];
                foreach (var log in logs)
                {
                    var employee = await employeeRepository.GetByIdAsync(log.CreatedById);
                    if (employee != null)
                    {
                        employeeLogPairs.Add(new Tuple<Log, Employee>(log, employee));
                    }
                }

                var viewModel = new TicketLogsViewModel(employeeLogPairs);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        private Employee? Authenticate()
        {
            var employee = Authorization.GetLoggedInEmployee(HttpContext);
            return employee is null or ServiceDeskEmployee ? null : employee;
        }

        private bool IsAuthenticated() => Authenticate() != null;
    }
}