using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceManager.Areas.Identity.Pages.Account.Manage
{
    public class CategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }
        public List<CategoryViewModel> ExpenseCategories { get; set; }
        public List<CategoryViewModel> IncomeCategories { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            var categories = await _context.Categories
                .Where(c => c.ApplicationUserId == userId)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color,
                    TransactionCount = _context.Transactions.Count(t => t.CategoryId == c.Id),
                    TotalAmount = _context.Transactions.Where(t => t.CategoryId == c.Id).Sum(t => t.Amount)
                })
                .ToListAsync();

            ExpenseCategories = categories.Where(c => _context.Categories.Any(cat => cat.Id == c.Id && cat.Type == CategoryType.Expense)).ToList();
            IncomeCategories = categories.Where(c => _context.Categories.Any(cat => cat.Id == c.Id && cat.Type == CategoryType.Income)).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var hasTransactions = await _context.Transactions.AnyAsync(t => t.CategoryId == id);
            if (hasTransactions)
            {
                StatusMessage = "Error: Cannot delete a category that is already in use by one or more transactions.";
                return RedirectToPage();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            StatusMessage = "Category has been deleted.";
            return RedirectToPage();
        }
    }
}