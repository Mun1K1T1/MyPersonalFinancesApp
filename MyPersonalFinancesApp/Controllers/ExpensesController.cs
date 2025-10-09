using Microsoft.AspNetCore.Mvc;
using MyPersonalFinancesApp.Data;

namespace MyPersonalFinancesApp.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly MyPersonalFinancesAppContext _context;
        public ExpensesController(MyPersonalFinancesAppContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var expenses = _context.Expenses.ToList();
            return View(expenses);
        }
    }
}
