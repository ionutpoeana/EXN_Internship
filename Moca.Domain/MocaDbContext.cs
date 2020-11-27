using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Moca.Domain.Entities;
using Moca.Domain.Properties;

#nullable disable

namespace Moca.Domain
{
    public class MocaDbContext : DbContext
    {

        public DbSet<Component> Components { get; set; }
        public DbSet<ComponentBrand> ComponentBrands { get; set; }
        public DbSet<ComponentHistory> ComponentHistories { get; set; }
        public DbSet<ComponentModel> ComponentModels { get; set; }
        public DbSet<ComponentRequest> ComponentRequests { get; set; }
        public DbSet<ComponentType> ComponentTypes { get; set; }
        public DbSet<DefectiveComponent> DefectiveComponents { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=LAPTOP-18\\SQLEXPRESS01;Initial Catalog=Moca;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"); //Resources.connectionString);
            }
        }
    }
}
