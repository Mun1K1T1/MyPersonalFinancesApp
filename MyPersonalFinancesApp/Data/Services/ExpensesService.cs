using Microsoft.EntityFrameworkCore;
using MyPersonalFinancesApp.Models;

namespace MyPersonalFinancesApp.Data.Services
{
    public class ExpensesService : IExpensesService
    {
        private readonly MyPersonalFinancesAppContext _context;
        public ExpensesService(MyPersonalFinancesAppContext context)
        {
            _context = context;
        }
        public async Task Add(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expense>> GetAll()
        {
            var expenses = await _context.Expenses.ToListAsync();
            return expenses;
        }

        public IQueryable GetChartData()
        {
            var data = _context.Expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount)
                });
            return data;
        }
    }
}
