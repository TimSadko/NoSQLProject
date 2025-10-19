

using NoSQLProject.Models;
using NoSQLProject.Repositories;
using NoSQLProject.Other;

namespace NoSQLProject.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // ✅ Get all employees (default unsorted)
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
            => await _employeeRepository.GetAllAsync();

        // ✅ Get single employee by ID
        public async Task<Employee?> GetEmployeeByIdAsync(string id)
            => await _employeeRepository.GetByIdAsync(id);

        // ✅ Get employee by email
        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
            => await _employeeRepository.GetByEmailAsync(email);

        // ✅ Add employee (with hashed password + email validation)
        public async Task AddEmployeeAsync(Employee employee)
        {
            var existing = await _employeeRepository.GetByEmailAsync(employee.Email);
            if (existing != null)
                throw new InvalidOperationException("An employee with this email already exists.");

            employee.Password = Hasher.GetHashedString(employee.Password);
            await _employeeRepository.Add(employee);
        }

        // ✅ Update employee (hashes new password if changed)
        public async Task UpdateEmployeeAsync(Employee employee)
        {
            var existing = await _employeeRepository.GetByIdAsync(employee.Id);
            if (existing == null) throw new InvalidOperationException("Employee not found.");

            if (!string.IsNullOrWhiteSpace(employee.Password) && employee.Password != existing.Password)
            {
                existing.Password = Hasher.GetHashedString(employee.Password);
            }

            existing.FirstName = employee.FirstName;
            existing.LastName = employee.LastName;
            existing.Email = employee.Email;
            existing.Status = employee.Status;

            if (existing is ServiceDeskEmployee sdeExisting && employee is ServiceDeskEmployee sdeInput)
            {
                sdeExisting.ManagedEmployees = sdeInput.ManagedEmployees;
            }

            await _employeeRepository.Update(existing);
        }

        // ✅ Delete employee
        public async Task DeleteEmployeeAsync(Employee employee)
            => await _employeeRepository.Delete(employee);

        // ✅ Get all employees managed by a specific Service Desk employee
        public async Task<List<Employee>> GetEmployeesManagedByAsync(string serviceDeskEmployeeId)
        {
            var sde = await _employeeRepository.GetByIdAsync(serviceDeskEmployeeId) as ServiceDeskEmployee;
            if (sde == null || sde.ManagedEmployees == null || !sde.ManagedEmployees.Any())
                return new List<Employee>();

            return await _employeeRepository.GetEmployeesByIdsAsync(sde.ManagedEmployees);
        }

        // ✅✅✅ Added by TAREK — Sorting functionality for Employees page (Assignment 2)
        // This calls the repository's dynamic sort method to allow sorting by any field.
        public async Task<List<Employee>> GetAllEmployeesSortedAsync(string sortField = "Status", int sortOrder = 1)
        {
            return await _employeeRepository.GetAllSortedAsync(sortField, sortOrder);
        }
        // ✅ END of TAREK’s sorting feature
    }
}
