using Microsoft.Data.SqlClient;
using Moca.Domain;
using Xunit;
using System.Configuration;
using System.Linq;
using Dapper;
using FluentAssertions;
using Moca.Tests.TestDtos;

namespace Moca.Tests
{
    public class TestMocaLinqQueries
    {
        private readonly MocaLinqQuerries _mocaLinqQuerries;

        private readonly string _connectionString =
            "Data Source=LAPTOP-18\\SQLEXPRESS01;Initial Catalog=Moca;Integrated Security=True;" +
            "Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;" +
            "ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public TestMocaLinqQueries()
        {
            //TODO: solve connection string storage problem in app.config
            _mocaLinqQuerries = new MocaLinqQuerries(new MocaDbContext());
        }

        [Fact]
        public void Test1()
        {
            // Arrange
            const string query = "SELECT ID, DaysInUse FROM ComponentHistories;";
            using var connection = new SqlConnection(_connectionString);
            
            // Act
            var sqlComponents = connection.Query<ComponentDaysInUse>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetComponentsDaysInUses();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

        [Fact]
        public void Given_Number_Of_Components_Out_Of_Use_LINQ_And_SQL_Queries_Results_Should_Be_Equal()
        {
            // Arrange
            const string query = "SELECT Count(dc.ID) FROM DefectiveComponents dc WHERE dc.ReparationStatus = 3; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<int>(query).SingleOrDefault();
            var linqComponents = _mocaLinqQuerries.GetComponentsOutOfUse();

            // Assert
            sqlComponents.Should().Equals(linqComponents);
        }

        [Fact]
        public void Given_Components_Stats_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent()
        {
            // Arrange
            const string query = "SELECT c.ID, ct.Name, MIN(ch.DaysInUse) AS Min, MAX(ch.DaysInUse) AS Max, AVG(CAST(ch.DaysInUse as float)) AS Average" +
               " FROM Components c" +
                   " inner join ComponentHistories ch ON c.ID = ch.ComponentID" +
               " inner join ComponentTypes ct ON ct.ID = c.TypeID " +
               " GROUP BY C.ID, ct.Name; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<ComponentStats>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetComponentStats();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);


            // TODO: should i return fraction days or full days at average?
        }


        [Theory]
        [InlineData(3)]
        public void Given_Employees_Number_Which_Are_Using_SubComponents_LINQ_And_SQL_Queries_Results_Should_Be_Equal(int role)
        {
            // Arrange
            const string query = "SELECT COUNT(*) AS EmployeesNumber"+
                " FROM Employees e" +
                    " inner join ComponentHistories ch ON e.ID = ch.EmployeeID" +
                " inner join Components c ON ch.ComponentID = c.ID" +
                " WHERE e.RoleID = 3 and c.ParentID is not null and ch.EndDate > GETDATE();";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<int>(query).SingleOrDefault();
            var linqComponents = _mocaLinqQuerries.GetNumberOfEmployeesWithSubComponents(role);

            // Assert
            sqlComponents.Should().Equals(linqComponents);
        }


        [Theory]
        [InlineData(3)]
        public void Given_Component_Brands_In_The_Past_Months_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(int months)
        {
            // Arrange
            const string query = "SELECT distinct cm.name, cb.Name as BrandName" +
                " FROM ComponentHistories ch" +
                   " inner join Components c ON ch.ComponentID = c.ID" +
                " inner join ComponentModels cm ON c.ModelID = cm.ID" +
                " inner join ComponentBrands cb ON cm.BrandID = cb.ID" +
                " WHERE ch.EndDate < DATEADD(MONTH, -3, GETDATE())" +
                " GROUP BY cm.name, cb.name; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<ComponentBrand>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetComponentsBrandByUseTime(months);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

        [Fact]
        public void Given_Number_Of_Independent_Components_LINQ_And_SQL_Queries_Results_Should_Be_Equal()
        {
            // Arrange
            const string query = " SELECT COUNT(*) " +
                                 " FROM DefectiveComponents dc" +
                                    " inner join Components c ON dc.ComponentID = c.ID" +
                                 " WHERE c.ParentID is null and not (exists(" +
                                         " SELECT 1" +
                                         " FROM Components c1" +
                                         " WHERE c.ID = c1.ParentID )); ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<int>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetIndependentDefectiveCompNum();

            // Assert
            sqlComponents.Should().Equal(linqComponents);
        }

        [Fact]
        public void Given_Count_Of_Components_By_Brand_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent()
        {
            // Arrange
            const string query = " SELECT cb.Name as BrandName, COUNT(c.ID) AS Count " +
                                 " FROM ComponentBrands cb " +
                                     " inner join ComponentModels cm ON cb.ID = cm.BrandID " +
                                     " inner join Components c ON cm.ID = c.ModelID " +
                                 " GROUP BY cb.Name " +
                                 " ORDER BY Count desc; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<ComponentsBrandCount>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetComponentsCountByBrand();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        } 
        
        
        [Theory]
        [InlineData("D","a")]
        public void Given_Employees_Name_By_Pattern_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(string startsWith, string endsWith)
        {
            // Arrange
            string query = "SELECT LastName, FirstName " +
                           " FROM Employees " +
                           $" WHERE LastName like '{startsWith}%' and(LastName like '%{endsWith}' or FirstName like '%{endsWith}'); ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeeFullName>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesByName(startsWith,endsWith);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }  
        
        
        [Theory]
        [InlineData(1)]
        public void Given_Amount_Spent_On_Components_LINQ_And_SQL_Queries_Results_Should_Be_Equal(int yearsInPast)
        {
            // Arrange
            string query = " SELECT SUM(EstimatedPrice) " +
                           " FROM ComponentRequests " +
                           $" WHERE Status = 1 and RequestDate > DATEADD(YEAR, {-yearsInPast}, SYSDATETIME())";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<double>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetComponentExpensesInPast(yearsInPast);

            // Assert
            sqlComponents.Should().Equal(linqComponents);
        }


        [Theory]
        [InlineData(5,8)]
        public void Given_Employees_By_Request_Period_LINQ_And_SQL_Queries_Results_Should_Be_Equal(int startMonth, int endMonth)
        {
            // Arrange
            string query = " SELECT e.FirstName, e.LastName, e.Email " +
                           " FROM Employees e " +
                           " inner join ComponentRequests cr ON e.ID = cr.EmployeeID " +
                           $" WHERE cr.Status = 1 and Month(cr.RequestDate) between {startMonth} and {endMonth};";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeeNameAndEmail>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesByRequestPeriod(startMonth,endMonth);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }
        
        
        [Fact]
        public void Given_Employees_Emails_By_Role_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent()
        {
            //TODO: with or without , in emails, or maybe a list

            // Arrange
            const string query = "SELECT r.Name as RoleName, '[' + STRING_AGG(e.email, ',') + ']' AS Emails " +
                                 " FROM Roles r " +
                                    " inner join Employees e ON r.ID = e.RoleID " +
                                 " GROUP BY r.Name; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeesMailByRole>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesMailByRoles();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

        [Theory]
        [InlineData(30)]
        public void Given_Top_N_Youngest_Employees_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(int top)
        {
            // Arrange
            string query = $"SELECT top {top} LastName, FirstName, YEAR(GETDATE()) - YEAR(DateOfBirth) AS Age " +
                                 " FROM Employees " +
                                 " ORDER BY  Age, LastName asc; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeeAge>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetTopEmployeesByAge(top);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

        [Theory]
        [InlineData(3)]
        public void Given_Employees_By_Component_Use_Time_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(int monthsUseTime)
        {
            // Arrange
            string query = "SELECT distinct LastName, (YEAR(GETDATE())- YEAR(e.DateOfBirth)) AS Age, e.Email " +
                                 " FROM Employees e " +
                                    " inner join ComponentHistories ch ON e.ID = ch.EmployeeID" +
                                 $" WHERE DATEDIFF(MONTH, ch.StartDate, ch.EndDate) >= {monthsUseTime}" +
                                 " ORDER BY  LastName; ";
            using var connection = new SqlConnection(_connectionString);

            //TODO: sql returns 12 more elements
            // Act
            var sqlComponents = connection.Query<EmployeeAgeAndEmail>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesByComponentUseTime(monthsUseTime);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }



        [Theory]
        [InlineData(10)]
        public void Given_Top_N_Employees_By_Request_Number_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(int top)
        {
            //TODO: invalid exception
            // Arrange
            string query = $"SELECT top {top} CONCAT_WS(' ', e.LastName, e.FirstName) AS Employee, " +
                               " (YEAR(SYSDATETIME()) - YEAR(DateOfBirth)) AS Age, " +
                               " count(cr.ID) AS Requests " +
                           " FROM Employees e " +
                              " inner join ComponentRequests cr ON e.ID = cr.EmployeeID " +
                           " GROUP BY e.FirstName, e.LastName, e.DateOfBirth " +
                           " ORDER BY count(cr.EmployeeID) desc, e.LastName; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeesRequest>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetTopEmployeesByRequestNumber(top);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

        [Fact]
        public void Given_Employees_Which_Use_Same_Brand_As_Colleague_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent()
        {
            // Arrange
            const string query = "SELECT DISTINCT	 CONCAT_WS(' ', e1.FirstName, e1.LastName) AS Employee " +
                                 " FROM Employees e1  " +
                                     " inner join Employees e2 ON e1.ID != e2.ID and e1.RoleID = e2.RoleID  " +

                                     " inner join ComponentHistories ch1 ON ch1.EmployeeID = e1.ID  " +
                                     " inner join Components c1 ON c1.ID = ch1.ComponentID          " +
                                     " inner join ComponentModels cm1 ON cm1.ID = c1.ModelID        " +

                                     " inner join ComponentHistories ch2 ON ch2.EmployeeID = e2.ID  " +
                                     " inner join Components c2 ON c2.ID = ch2.ComponentID          " +
                                     " inner join ComponentModels cm2 ON cm2.ID = c2.ModelID        " +
                                 " WHERE cm1.BrandID = cm2.BrandID                              " +
                                 " GROUP BY e1.RoleID, e1.FirstName, e1.LastName                " +
                                 "     ORDER BY Employee; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<string>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesWhichUsedSameBrand();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }


        [Theory]
        [InlineData(35, 45)]
        public void Given_Employees_By_Age_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(int lowerAgeLimit, int higherAgeLimit)
        {
            // Arrange
            string query = "SELECT FirstName, LastName, (YEAR(SYSDATETIME())- YEAR(DateOfBirth)) AS Age " +
                           " FROM Employees " +
                           $" WHERE DATEDIFF(YEAR, DateOfBirth, GETDATE()) between {lowerAgeLimit}  and {higherAgeLimit} " +
                           " ORDER BY  LEN(FirstName), LEN(LastName); ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeeAge>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesBetweenAge(lowerAgeLimit,higherAgeLimit);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

        [Fact]
        public void Given_Employees_With_BirthDate_In_Same_Year_And_Month_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent()
        {
            // Arrange
            const string query = "SELECT distinct CONCAT_WS('_',e1.FirstName,e1.LastName) AS Employee " +
                                 " FROM Employees e1 " +
                                 "     inner join Employees e2 ON e1.RoleID = e2.RoleID and e1.ID != e2.ID" +
                                 " WHERE YEAR(e1.DateOfBirth) = YEAR(e2.DateOfBirth) and " +
                                 " MONTH(e1.DateOfBirth) = MONTH(e2.DateOfBirth)" +
                                 " ORDER BY  Employee;";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<string>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesByBirthAgeAndMonth();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }


        [Theory]
        [InlineData(5)]
        public void Given_Component_Expenses_In_Past_N_Years_By_Employee_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent(int yearsInPast)
        {
            // Arrange
            string query = " SELECT CONCAT_WS(' ',e.FirstName, e.LastName, e.Email) AS EmployeeDetails, " +
                                " ISNULL(sum(cr.EstimatedPrice), 0) AS ComponentExpenses " +
                           " FROM Employees e " +
                                " left outer join ComponentRequests cr ON e.ID = cr.EmployeeID " +
                           $" WHERE cr.Status != 3 and cr.RequestDate > DATEADD(YEAR,{-yearsInPast}, RequestDate)" +
                           " GROUP BY e.ID, e.LastName, e.FirstName, e.Email" +
                           " ORDER BY ComponentExpenses";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeeExpenses>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeesComponentRequests(yearsInPast);

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }


        [Fact]
        public void Given_Employees_With_Full_Name_Like_Pattern_LINQ_And_SQL_Queries_Results_Should_Be_Equivalent()
        {
            // Arrange
            const string query = " SELECT FirstName, LastName " +
                                 " FROM Employees " +
                                 " WHERE LastName LIKE '[AEIOU]%[BCDFGHJKLMNPQRSTVWXYZ]' " +
                                 " ORDER BY  DateOfBirth; ";
            using var connection = new SqlConnection(_connectionString);

            // Act
            var sqlComponents = connection.Query<EmployeeFullName>(query).ToList();
            var linqComponents = _mocaLinqQuerries.GetEmployeeFullNamesByPattern();

            // Assert
            sqlComponents.Should().BeEquivalentTo(linqComponents);
        }

    } 
}