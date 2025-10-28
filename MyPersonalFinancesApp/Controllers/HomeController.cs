using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManager.Controllers
{
    [Authorize] // Require user to be logged in to see the dashboard
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // The 'accountId' parameter will come from the dropdown filter
        public async Task<IActionResult> Index(int? accountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAccounts = await _context.Accounts
                .Where(a => a.ApplicationUserId == userId && a.IsActive)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                SelectedAccountId = accountId,
                Accounts = new SelectList(userAccounts, "Id", "Name", accountId)
            };

            //Transactions for the current user
            var transactionsQuery = _context.Transactions.Where(t => t.Account.ApplicationUserId == userId);

            // If a specific account is selected, filter by it
            if (accountId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.AccountId == accountId.Value);
            }

            // Get Recent Incomes & Expenses (Last 10)
            viewModel.RecentIncomes = await transactionsQuery.OfType<Income>()
                .Include(i => i.Category)
                .OrderByDescending(i => i.Date)
                .Take(10)
                .ToListAsync();

            viewModel.RecentExpenses = await transactionsQuery.OfType<Expense>()
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date)
                .Take(10)
                .ToListAsync();

            // Prepare Expense Chart Data
            var expenseData = await transactionsQuery.OfType<Expense>()
                .Include(e => e.Category)
                .GroupBy(e => new { e.Category.Name, e.Category.Color }) // Group by Color
                .Select(group => new
                {
                    Category = group.Key.Name,
                    Color = group.Key.Color,
                    Total = group.Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            viewModel.ExpenseChartData.Labels = expenseData.Select(x => x.Category).ToList();
            viewModel.ExpenseChartData.Values = expenseData.Select(x => x.Total).ToList();
            viewModel.ExpenseChartData.Colors = expenseData.Select(x => x.Color).ToList();

            // Prepare Income Chart Data
            var incomeData = await transactionsQuery.OfType<Income>()
                .Include(i => i.Category)
                .GroupBy(i => new { i.Category.Name, i.Category.Color }) // Group by Color
                .Select(group => new
                {
                    Category = group.Key.Name,
                    Color = group.Key.Color,
                    Total = group.Sum(i => i.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            viewModel.IncomeChartData.Labels = incomeData.Select(x => x.Category).ToList();
            viewModel.IncomeChartData.Values = incomeData.Select(x => x.Total).ToList();
            viewModel.IncomeChartData.Colors = incomeData.Select(x => x.Color).ToList();

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}