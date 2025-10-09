using Microsoft.EntityFrameworkCore;
using MyPersonalFinancesApp.Models;

namespace MyPersonalFinancesApp.Data
{
    public class PersonalFinanceAppContext : DbContext
    {
        public PersonalFinanceAppContext(DbContextOptions<PersonalFinanceAppContext> options):base(options) { }

        DbSet<Expence> Expences { get; set; }
    }
}
