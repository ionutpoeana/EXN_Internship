using System;
using System.Collections.Generic;
using System.Linq;
using Moca.Tests.TestDtos;

namespace Moca.Domain
{
    public class MocaLinqQuerries
    {
        private readonly MocaDbContext _context;

        public MocaLinqQuerries(MocaDbContext context)
        {
            _context = context;
        }

        
        /// <summary>
        ///  Gets all employees with a colleague (an employee with the same role) which
        /// made a request for component from the same brand
        /// </summary>
        /// <returns>List of employees that used a component of same brand</returns>
        public IList<string> GetEmployeesWhichUsedSameBrand()
        {


            var employees = from e1 in _context.Employees
                            join e2 in _context.Employees on e1.ID! equals e2.ID
                            orderby e1.LastName
                            where e1.RoleId == e2.RoleId &&
                                  e1.ComponentHistories.Any(ch =>
                                      e2.ComponentHistories.Any(ch2 =>
                                          ch2.Component.Model.BrandId == ch.Component.Model.BrandId))
                            select
                                string.Join(' ', e1.FirstName, e1.LastName);

            return employees.ToList();
        }

        /// <summary>
        /// Gets the name and surname of employees with surname starting with a consonant and finishing with
        /// a vowel order by date of birth
        /// </summary>
        /// <returns>List of employees</returns>
        public IList<EmployeeFullName> GetEmployeeFullNamesByPattern()
        {
            var employees = _context.Employees
                .Where(e => (e.LastName.StartsWith("a")  ||
                            e.LastName.StartsWith("e") ||
                            e.LastName.StartsWith("i") ||
                            e.LastName.StartsWith("o") ||
                            e.LastName.StartsWith("u")) &&

                            !( e.LastName.EndsWith("a") ||
                              e.LastName.EndsWith("e") ||
                              e.LastName.EndsWith("i") ||
                              e.LastName.EndsWith("o") ||
                              e.LastName.EndsWith("u") ) )
                .OrderBy(e => e.DateOfBirth)
                .Select(e => new EmployeeFullName
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName
                });

            return employees.ToList();
        }

        /// <summary>
        /// Get all the employees (Name, Surname and Mail in a column 'EmployeeDetails'), the total cost of
        /// component requests (both approved and not) in the last years. If no requests were made, Get  0.
        /// </summary>
        /// <param name="yearsInPast"> number of years in the past compared to current year</param>
        /// <returns>A list of employees with their component expenses</returns>
        public IList<EmployeeExpenses> GetEmployeesComponentRequests(int yearsInPast)
        {
            var employees = _context.Employees
                .Select(e => new EmployeeExpenses
                {
                    EmployeeDetails = string.Join(' ', e.FirstName, e.LastName, e.Email),
                    ComponentExpenses = e.ComponentRequests
                        .Where(cr => DateTime.Now.Year - cr.RequestDate.Year > yearsInPast)
                        .Sum(cr => cr.EstimatedPrice)
                }).OrderBy(e => e.ComponentExpenses);

            return employees.ToList();
        }


        /// <summary>
        /// Get the Name and Surname separated by '_' for all employees with at least a
        /// colleague (same role) with the date of birth in the same month and year</summary>
        /// <returns>A list of employees</returns>
        public IList<string> GetEmployeesByBirthAgeAndMonth()
        {

            var filteredEmployees = from e in _context.Employees
                join c in _context.Employees on e.RoleId equals c.RoleId
                where e.ID != c.ID && e.DateOfBirth.Year == c.DateOfBirth.Year &&
                      e.DateOfBirth.Month == c.DateOfBirth.Month
                select e.FirstName + "_" + e.LastName;

            return filteredEmployees.OrderBy(e => e).ToList();
        }

        /// <summary>
        /// Get  Name, Surname and Age for all employees with age between lowerAgeLimit and higherAgeLimit.
        /// Order the list by the length of the name and surname.
        /// </summary>
        public IList<EmployeeAge> GetEmployeesBetweenAge(int lowerAgeLimit, int higherAgeLimit)
        {

            var employees = _context.Employees
                .Where(e =>
                    e.DateOfBirth < DateTime.Now.AddYears(-lowerAgeLimit) &&
                    e.DateOfBirth > DateTime.Now.AddYears(-higherAgeLimit))
                .Select(e => new EmployeeAge
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Age = DateTime.Now.Year - e.DateOfBirth.Year
                })
                .OrderBy(e => e.FirstName.Length)
                .ThenBy(e => e.LastName.Length);

            return employees.ToList();
        }

        /// <summary>
        /// Get  the full name in column 'Employee' and the age in column 'Age' for the top employeNumber employees
        /// with the biggest number of requests.
        /// </summary>
        /// <param name="employeesNumber"></param>
        /// <returns>A list of EmployeeRequests ordered by requestCount and name</returns>
        public IList<EmployeesRequest> GetTopEmployeesByRequestNumber(int employeesNumber)
        {
            var employees = _context.Employees
                .OrderByDescending(e => e.ComponentRequests.Count)
                .ThenBy(e => e.LastName)
                .Take(employeesNumber)
                .Select(e => new EmployeesRequest
                {
                    Employee = e.LastName + " " +  e.FirstName ,
                    Age = DateTime.Now.Year - e.DateOfBirth.Year,
                    Requests = e.ComponentRequests.Count
                });

            return employees.OrderBy(e => e.Employee).ToList();
        }

        /// <summary>
        /// Get  the age and mail (alphabetically ordered by name) for each employee
        /// which is using a component for at least n months
        /// </summary>
        /// <param name="componentUseTime">Number of months for which a employee used a component</param>
        /// <returns>List of employees last name and email</returns>
        public IList<EmployeeAgeAndEmail> GetEmployeesByComponentUseTime(int componentUseTime)
        {


            var employees = _context.ComponentHistories
                .Where(ch => ch.StartDate < ch.EndDate.AddMonths(-componentUseTime))
                .Select(ch => new EmployeeAgeAndEmail
                {
                    LastName = ch.Employee.LastName,
                    Age = DateTime.Now.Year - ch.Employee.DateOfBirth.Year,
                    Email = ch.Employee.Email
                })
                .Distinct()
                .OrderBy(e => e.LastName);


            return employees.ToList();
        }

        /// <summary>
        /// Get top n youngest employees.
        /// </summary>
        /// <param name="numberOfEmployees">Maximum number of employees to be returned</param>
        /// <returns>List of employees name and age</returns>
        public IList<EmployeeAge> GetTopEmployeesByAge(int numberOfEmployees)
        {
            var employees = _context.Employees
                .Select(e => new EmployeeAge
                {
                    LastName = e.LastName,
                    FirstName = e.FirstName,
                    Age = DateTime.Now.Year - e.DateOfBirth.Year
                })
                .OrderBy(e => e.Age)
                .ThenBy(e=>e.LastName)
                .Take(numberOfEmployees);

            return employees.ToList();
        }

        /// <summary>
        /// For each role, Get  the name of the role and between brackets the mail for all employees with
        /// the current role separated by ','.</summary>
        /// <returns>List of roles and employees emails grouped by roles</returns>
        public IList<EmployeesMailByRole> GetEmployeesMailByRoles()
        {
            var roles = _context.Roles
                .Select(r => new EmployeesMailByRole
                {
                    RoleName = r.Name,
                    Emails = $"[{string.Join(',', r.Employees.Select(e => e.Email))}]"
                });

            return roles.ToList();
        }

        /// <summary>
        /// Get the Name, Mail and Phone number for all employees with a complete request between
        /// May and August last year.
        /// </summary>
        /// <param name="startMonth">index of start month</param>
        /// <param name="endMonth">index of stop month</param>
        /// <returns>List of employees full name and email</returns>
        public IList<EmployeeNameAndEmail> GetEmployeesByRequestPeriod(int startMonth, int endMonth)
        {
            var employees = _context.ComponentRequests
                .Where(cr => cr.Status == 1 &&
                             cr.RequestDate.Month >= startMonth &&
                             cr.RequestDate.Month <= endMonth)
                .Select(cr => new EmployeeNameAndEmail
                {
                    FirstName = cr.Employee.FirstName,
                    LastName = cr.Employee.LastName,
                    Email = cr.Employee.Email
                })
                .Distinct();

            return employees.ToList();
        }

        /// <summary>
        /// Get the total amount of spent money in the last year on components(approved requests)
        /// </summary>
        /// <param name="pastYears">Number of years in the past</param>
        /// <returns>Total amount spent on components last year</returns>
        public decimal GetComponentExpensesInPast(int pastYears)
        {
            var componentMoney = _context.ComponentRequests
                .Where(cr => cr.Status == 1 &&
                             cr.RequestDate > DateTime.Now.AddYears(-pastYears))
                .Sum(cr => cr.EstimatedPrice);
            return componentMoney;
        }

        /// <summary>
        /// Get the full name of employees with the name starting
        /// with 'D' and at least a surname is ending in 'a'.
        /// </summary>
        /// <param name="startsWith">Starting characters for employee surname</param>
        /// <param name="endsWith">Ending characters for employee surname or first name</param>
        /// <returns>list of employee name that matches rule</returns>
        public IList<EmployeeFullName> GetEmployeesByName(string startsWith, string endsWith)
        {
            var employees = _context.Employees
                .Where(e => e.LastName.StartsWith(startsWith) &&
                            (e.LastName.EndsWith(endsWith) || e.FirstName.EndsWith(endsWith)))
                .Select(e => new EmployeeFullName
                {
                    LastName = e.LastName,
                    FirstName = e.FirstName
                });

            return employees.ToList();
        }

        /// <summary>
        /// Get the brand name and the number of components in each brand
        /// order by number of components descending
        /// </summary>
        /// <returns>A List of components counted by brand</returns>
        public IList<ComponentsBrandCount> GetComponentsCountByBrand()
        {
            // couldn't use aggregate error
            var components = _context.Components
                .Select(c => new { c.Model.Brand.Name, Count = 1 })
                .ToList()
                .GroupBy(c => c.Name)
                .Select(c => new ComponentsBrandCount
                {
                    BrandName = c.Key,
                    Count = c.Count()
                })
                .OrderByDescending(c => c.Count);

            return components.ToList();

        }

        /// <summary>
        ///  Get the number of defective independent components
        /// </summary>
        /// <returns>Number of independent defective components</returns>
        public int GetIndependentDefectiveCompNum()
        {
            var defectIndpComp = _context.DefectiveComponents
                .Count(dc => dc.Component.ParentId == null &&
                             !dc.Component.InverseParent.Any());

            return defectIndpComp;
        }

        /// <summary>
        /// Get the distinct Name and Brand of the components with the last
        /// use time lower than current date  - months number
        /// </summary>
        /// <param name="lastUseTime">Last use time in months relative to current date</param>
        /// <returns>List of component brands by use time</returns>
        public IList<ComponentBrand> GetComponentsBrandByUseTime(int lastUseTime)
        {

            var brandAndName = _context.Components
                .Where(c => c.ComponentHistories.Max(ch =>
                    ch.EndDate) < DateTime.Now.AddMonths(-lastUseTime))
                .Select(c => new ComponentBrand
                {
                    Name = c.Model.Name,
                    BrandName = c.Model.Brand.Name
                })
                .Distinct();

            return brandAndName.ToList();
        }

        /// <summary>
        /// Get the number of employees with role which are currently using a sub component
        /// </summary>
        /// <param name="role">Id of employees role</param>
        /// <returns>Number of employees that uses a sub component</returns>
        public int GetNumberOfEmployeesWithSubComponents(int role)
        {
            var employeesWithSubComponents = _context.Employees
                .Where(e => e.RoleId == role)
                .Count(e => e.ComponentHistories.Any(ch =>
                    ch.Component.ParentId != null &&
                    ch.EndDate > DateTime.Now));

            return employeesWithSubComponents;
        }

        /// <summary>
        /// Get the average cost of component requests in the past in years time.
        /// </summary>
        /// <param name="yearsInPast">Number of years in the past relative at current date</param>
        /// <returns>Average cost of components in yearsInPast time</returns>
        public decimal GetAverageCostOfComponents(int yearsInPast)
        {
            var averageRequestCost = _context.ComponentRequests
                .Where(ch => ch.RequestDate > DateTime.Now.AddYears(-yearsInPast))
                .Average(ch => ch.EstimatedPrice);

            return averageRequestCost;
        }

        /// <summary>
        /// Get the minim, maxim and the average time of use of a component.
        /// It will be Get ed: ID, Name, Max, Min, Average.</summary>
        /// <returns>A list of component statistics</returns>
        public IList<ComponentStats> GetComponentStats()
        {
            var componentsUseStats = _context.Components
                .Select(c => new ComponentStats
                {
                    ID = c.ID,
                    Name = c.Type.Name,
                    Max = c.ComponentHistories.Max(ch => ch.DaysInUse),
                    Min = c.ComponentHistories.Min(ch => ch.DaysInUse),
                    Average = (float)c.ComponentHistories.Average(ch => ch.DaysInUse)
                });

            return componentsUseStats.ToList();
        }

        /// <summary>
        /// Get the number of components which as marked as permanently out of use.
        /// </summary>
        /// <returns>Number of components out of use ( which can not be repaired )</returns>
        public int GetComponentsOutOfUse() =>
            _context.DefectiveComponents.Count(dc => dc.ReparationStatus == 3);


        /// <summary>
        /// Get days in use for all components
        /// </summary>
        /// <returns>List components days in use </returns>
        public IList<ComponentDaysInUse> GetComponentsDaysInUses()
        {
            var componentsDaysInUse = _context.ComponentHistories
                .Select(ch => new ComponentDaysInUse
                {
                    ID = ch.ID,
                    DaysInUse = ch.DaysInUse
                });

            return componentsDaysInUse.ToList();
        }
    }
}