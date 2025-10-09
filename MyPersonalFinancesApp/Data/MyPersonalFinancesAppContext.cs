using Microsoft.EntityFrameworkCore;
using MyPersonalFinancesApp.Models;

namespace MyPersonalFinancesApp.Data
{
    public class MyPersonalFinancesAppContext : DbContext
    {
        public MyPersonalFinancesAppContext(DbContextOptions<MyPersonalFinancesAppContext> options):base(options) { }

        public DbSet<Expense> Expenses { get; set; }
    }
}
