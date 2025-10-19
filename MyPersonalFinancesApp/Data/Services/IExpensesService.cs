using MyPersonalFinancesApp.Models;

namespace MyPersonalFinancesApp.Data.Services
{
    public interface IExpensesService
    {
        Task<IEnumerable<Expense>> GetAll();
        Task Add(Expense expense);
        IQueryable GetChartData();
    }
}
