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

            // Base query for transactions for the current user
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
                .GroupBy(e => e.Category.Name)
                .Select(group => new { Category = group.Key, Total = group.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            viewModel.ExpenseChartData.Labels = expenseData.Select(x => x.Category).ToList();
            viewModel.ExpenseChartData.Values = expenseData.Select(x => x.Total).ToList();

            // Prepare Income Chart Data
            var incomeData = await transactionsQuery.OfType<Income>()
                .GroupBy(i => i.Category.Name)
                .Select(group => new { Category = group.Key, Total = group.Sum(i => i.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            viewModel.IncomeChartData.Labels = incomeData.Select(x => x.Category).ToList();
            viewModel.IncomeChartData.Values = incomeData.Select(x => x.Total).ToList();

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