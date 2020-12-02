using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Moca.Domain;
using Moca.Domain.Entities;

namespace Moca
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var context = new MocaDbContext();


            //SeedDatabase(context);

            var linqQueries = new MocaLinqQueries(context);
        }

        private static void SeedDatabase(MocaDbContext context)
        {
            var random = new Random();

            SeedComponentBrands(context);
            SeedComponentModels(context, random);
            SeedRoles(context);
            SeedEmployees(context, random);
            SeedComponentType(context);
            SeedComponents(context, random);
            SeedComponentsRequests(context, random);
            SeedDefectiveComponents(context, random);
            SeedComponentHistory(context, random);
        }

        private static void SeedComponentHistory(MocaDbContext context, Random random)
        {
            if (context.ComponentHistories.Any()) return;

            var historyFaker = new Faker<ComponentHistory>()
                .RuleFor(h => h.Observations, f => f.Lorem.Text());

            var components = context.Components.ToList();
            var employees = context.Employees.ToList();

            foreach (var component in components)
            {
                var historyRecords = historyFaker.Generate(random.Next(5, 30));
                var startDate = component.PurchaseDate.AddDays(random.Next(0, 10));
                var daysCount = (DateTime.Now - startDate).Days / historyRecords.Count;

                foreach (var record in historyRecords)
                {
                    record.Component = component;
                    record.Employee = employees[random.Next(0, employees.Count - 1)];
                    record.StartDate = startDate;
                    var endDate = startDate.AddDays(random.Next(0, daysCount));
                    record.EndDate = endDate;
                    startDate = endDate.AddDays(random.Next(0, daysCount));
                }

                context.AddRange(historyRecords);
            }

            context.SaveChanges();
        }

        private static void SeedDefectiveComponents(MocaDbContext context, Random random)
        {
            if (context.DefectiveComponents.Any()) return;

            var defectiveFaker = new Faker<DefectiveComponent>()
                .RuleFor(r => r.RequestDate, f => f.Date.Recent(30, DateTime.Now))
                .RuleFor(r => r.Observations, f => f.Lorem.Text());

            var components = context.Components.ToList();
            var employees = context.Employees.ToList();
            var defectiveComponents = defectiveFaker.Generate(10);

            foreach (var defective in defectiveComponents)
            {
                defective.Component = components[random.Next(0, components.Count - 1)];
                defective.Employee = employees[random.Next(0, employees.Count - 1)];
                defective.ReparationStatus = random.Next(0, Enum.GetNames(typeof(ReparationStatus)).Length);
            }

            context.AddRange(defectiveComponents);
            context.SaveChanges();
        }

        private static void SeedComponentsRequests(MocaDbContext context, Random random)
        {
            if (context.ComponentRequests.Any()) return;

            var componentRequest = new Faker<ComponentRequest>()
                .RuleFor(r => r.RequestDate, f => f.Date.Past(1, DateTime.Now))
                .RuleFor(r => r.Observations, f => f.Lorem.Text())
                .RuleFor(r => r.EstimatedPrice, f => f.Random.Decimal());

            var requests = componentRequest.Generate(40);
            var types = context.ComponentTypes.ToList();
            var employees = context.Employees.ToList();
            foreach (var request in requests)
            {
                request.Type = types[random.Next(0, types.Count)];
                request.Employee = employees[random.Next(0, employees.Count - 1)];
                request.Status = random.Next(0, 2);

            }
            context.AddRange(requests);
            context.SaveChanges();
        }

        private static void SeedComponents(MocaDbContext context, Random random)
        {
            if (context.Components.Any()) return;

            var componentsFaker = new Faker<Component>()
                .RuleFor(c => c.PurchaseDate, f => f.Date.Past(5, DateTime.Today));

            var componentList = componentsFaker.Generate(300);
            var types = context.ComponentTypes.ToList();
            var models = context.ComponentModels.ToList();

            foreach (var component in componentList)
            {
                component.Model = models[random.Next(0, models.Count - 1)];
                component.Type = types[random.Next(0, types.Count - 1)];
                component.IsDeleted = false;
            }

            context.Components.AddRange(componentList);
            context.SaveChanges();


            var components = context.Components.ToList();
            foreach (var comp in components)
            {
                if (random.NextDouble() < 0.3) continue;

                var randomCompId = components[random.Next(0, components.Count - 1)].ID;
                if (comp.ID != randomCompId)
                {
                    comp.ParentId = randomCompId;
                }
            }

            context.SaveChanges();
        }

        private static void SeedComponentType(MocaDbContext context)
        {
            if (context.ComponentTypes.Any()) return;

            var componentType = new Faker<ComponentType>()
                .RuleFor(c => c.Name, f => f.Commerce.ProductName());
            context.ComponentTypes.AddRange(componentType.Generate(50));
            context.SaveChanges();
        }

        private static void SeedEmployees(MocaDbContext context, Random random)
        {
            if (context.Employees.Any()) return;

            var employeeFaker = new Faker<Employee>("ro")
                .RuleFor(e => e.FirstName, f => f.Name.FirstName())
                .RuleFor(e => e.LastName, f => f.Name.LastName())
                .RuleFor(e => e.Email, (f, e) => f.Internet.ExampleEmail(e.FirstName, e.LastName))
                .RuleFor(e => e.TelephoneNumber, f => f.Phone.PhoneNumber("+40 ### ### ###"))
                .RuleFor(e => e.Address, f => f.Address.FullAddress())
                .RuleFor(e => e.HireDate, f => f.Date.Past(f.Random.Number(10), DateTime.Now));

            var employeesFakers = employeeFaker.Generate(100);

            var roles = context.Roles.ToList();

            foreach (var employee in employeesFakers)
            {
                var roleDistribution = random.NextDouble();
                employee.Role = roleDistribution switch
                {
                    double n when n < 0.1 => roles.FirstOrDefault(r => r.Name == "Admin"),
                    double n when n < 0.2 => roles.FirstOrDefault(r => r.Name == "Intern"),
                    double n when n < 0.4 => roles.FirstOrDefault(r => r.Name == "Finance"),
                    double n when n < 0.7 => roles.FirstOrDefault(r => r.Name == "Developer"),
                    double n when n < 0.9 => roles.FirstOrDefault(r => r.Name == "Quality Assurance"),
                    double n when n < 1 => roles.FirstOrDefault(r => r.Name == "Human Relations"),
                    _ => employee.Role
                };

                if (roleDistribution < 0.3)
                {
                    employee.ContractEndDate = null;
                    continue;
                }

                employee.ContractEndDate = employee.HireDate.AddYears(random.Next(1, 7));

            }

            context.Employees.AddRange(employeesFakers);
            context.SaveChanges();
        }

        private static void SeedRoles(MocaDbContext context)
        {
            if (context.Roles.Any()) return;

            var roles = new List<Role>
            {
                new Role
                {
                    Name = "Admin",
                    Description = "Gives everything Moca"
                },
                new Role
                {
                    Name = "Finance",
                    Description = "Wonders why 'Admin' gave everything Moca"
                },
                new Role
                {
                    Name = "Developer",
                    Description = "Center of his Earth. Need's everything Moca"
                },
                new Role
                {
                    Name = "Quality Assurance",
                    Description = "Is the developer center of his Earth :-??. Also need's everything Moca"
                },
                new Role
                {
                    Name = "Human Relations",
                    Description = "Some angels when you don't ask for more money"
                },
                new Role
                {
                    Name = "Intern",
                    Description = "Lowest that can exist. We needed a new bottom line. But hey, everyone needs to start somewere"
                }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        private static void SeedComponentModels(MocaDbContext context, Random random)
        {
            if (context.ComponentModels.Any()) return;

            var componentModel = new Faker<ComponentModel>()
                .RuleFor(c => c.Name, f => f.Commerce.ProductName())
                .RuleFor(c => c.Description, f => f.Lorem.Text());

            var modelList = componentModel.Generate(50);
            var brandList = context.ComponentBrands.ToList();

            foreach (var model in modelList)
            {
                model.BrandId = brandList[random.Next(0, brandList.Count - 1)].ID;
            }

            context.ComponentModels.AddRange(modelList);
            context.SaveChanges();
        }

        private static void SeedComponentBrands(MocaDbContext context)
        {
            if (context.ComponentBrands.Any()) return;

            var componentBrand = new Faker<ComponentBrand>()
                .RuleFor(m => m.Name, f => f.Company.CompanyName());

            context.ComponentBrands.AddRange(componentBrand.Generate(50));
            context.SaveChanges();
        }
    }
}
