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

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
            => await _employeeRepository.GetAllAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(string id)
            => await _employeeRepository.GetByIdAsync(id);

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
            => await _employeeRepository.GetByEmailAsync(email);

        public async Task AddEmployeeAsync(Employee employee)
        {
            var existing = await _employeeRepository.GetByEmailAsync(employee.Email);
            if (existing != null)
                throw new InvalidOperationException("An employee with this email already exists.");

            employee.Password = Hasher.GetHashedString(employee.Password);
            await _employeeRepository.Add(employee);
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            var existing = await _employeeRepository.GetByIdAsync(employee.Id);
            if (existing == null) throw new InvalidOperationException("Employee not found.");

            // Only update the password if a new one is provided
            if (!string.IsNullOrWhiteSpace(employee.Password) && employee.Password != existing.Password)
            {
                existing.Password = Hasher.GetHashedString(employee.Password);
            }

            // Update other fields
            existing.FirstName = employee.FirstName;
            existing.LastName = employee.LastName;
            existing.Email = employee.Email;
            existing.Status = employee.Status;

            // If ServiceDeskEmployee, update managed employees as well
            if (existing is ServiceDeskEmployee sdeExisting && employee is ServiceDeskEmployee sdeInput)
            {
                sdeExisting.ManagedEmployees = sdeInput.ManagedEmployees;
            }

            await _employeeRepository.Update(existing);
        }

        public async Task DeleteEmployeeAsync(Employee employee)
            => await _employeeRepository.Delete(employee);

        public async Task<List<Employee>> GetEmployeesManagedByAsync(string serviceDeskEmployeeId)
        {
        var sde = await _employeeRepository.GetByIdAsync(serviceDeskEmployeeId) as ServiceDeskEmployee;
        if (sde == null || sde.ManagedEmployees == null || !sde.ManagedEmployees.Any())
        return new List<Employee>();

        return await _employeeRepository.GetEmployeesByIdsAsync(sde.ManagedEmployees);
        }

        public async Task<List<Employee>> GetEmployeesByStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status))
                return (await _employeeRepository.GetAllAsync()).ToList();

            return await _employeeRepository.GetByStatusAggregationAsync(status);
        }
    }
}