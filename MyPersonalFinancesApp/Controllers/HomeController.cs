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
using Microsoft.AspNetCore.Localization;

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

        public async Task<IActionResult> Index(int? accountId, TimePeriod selectedTimePeriod = TimePeriod.AllTime)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAccounts = await _context.Accounts
                .Where(a => a.ApplicationUserId == userId && a.IsActive)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                SelectedAccountId = accountId,
                Accounts = new SelectList(userAccounts, "Id", "Name", accountId),
                SelectedTimePeriod = selectedTimePeriod // The received value
            };

            // --- CURRENT AMOUNT CALCULATION ---
            IQueryable<Transaction> balanceQuery = _context.Transactions.Where(t => t.Account.ApplicationUserId == userId);
            if (accountId.HasValue)
            {
                balanceQuery = balanceQuery.Where(t => t.AccountId == accountId.Value);
            }

            var totalIncome = await balanceQuery.OfType<Income>().SumAsync(i => (decimal?)i.Amount) ?? 0;
            var totalExpense = await balanceQuery.OfType<Expense>().SumAsync(e => (decimal?)e.Amount) ?? 0;
            viewModel.CurrentAmount = totalIncome - totalExpense;

            // --- CHART AND HISTORY QUERY ---
            IQueryable<Transaction> filteredQuery = _context.Transactions.Where(t => t.Account.ApplicationUserId == userId);
            if (accountId.HasValue)
            {
                filteredQuery = filteredQuery.Where(t => t.AccountId == accountId.Value);
            }

            DateTime startDate = DateTime.MinValue;
            switch (selectedTimePeriod)
            {
                case TimePeriod.Day:
                    startDate = DateTime.Today;
                    break;
                case TimePeriod.Week:
                    startDate = DateTime.Today.AddDays(-6);
                    break;
                case TimePeriod.Month:
                    startDate = DateTime.Today.AddMonths(-1);
                    break;
                case TimePeriod.Year:
                    startDate = DateTime.Today.AddYears(-1);
                    break;
            }

            if (selectedTimePeriod != TimePeriod.AllTime)
            {
                filteredQuery = filteredQuery.Where(t => t.Date >= startDate);
            }

            // Get Total Counts BEFORE taking the top 20
            viewModel.TotalIncomeCountForPeriod = await filteredQuery.OfType<Income>().CountAsync();
            viewModel.TotalExpenseCountForPeriod = await filteredQuery.OfType<Expense>().CountAsync();

            viewModel.RecentIncomes = await filteredQuery.OfType<Income>()
                .Include(i => i.Category)
                .OrderByDescending(i => i.Date)
                .Take(20)
                .ToListAsync();

            viewModel.RecentExpenses = await filteredQuery.OfType<Expense>()
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date)
                .Take(20)
                .ToListAsync();

            var expenseData = await filteredQuery.OfType<Expense>()
                .Include(e => e.Category)
                .GroupBy(e => new { e.Category.Name, e.Category.Color })
                .Select(group => new { Category = group.Key.Name, Color = group.Key.Color, Total = group.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            viewModel.ExpenseChartData.Labels = expenseData.Select(x => x.Category).ToList();
            viewModel.ExpenseChartData.Values = expenseData.Select(x => x.Total).ToList();
            viewModel.ExpenseChartData.Colors = expenseData.Select(x => x.Color).ToList();

            var incomeData = await filteredQuery.OfType<Income>()
                .Include(i => i.Category)
                .GroupBy(i => new { i.Category.Name, i.Category.Color })
                .Select(group => new { Category = group.Key.Name, Color = group.Key.Color, Total = group.Sum(i => i.Amount) })
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

        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}