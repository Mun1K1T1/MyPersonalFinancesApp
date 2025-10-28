using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManager.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(type) || (type != "Income" && type != "Expense"))
            {
                type = "Expense";
            }

            var viewModel = new TransactionCreateViewModel
            {
                TransactionType = type,
                Accounts = new SelectList(await _context.Accounts
                    .Where(a => a.ApplicationUserId == userId && a.IsActive)
                    .ToListAsync(), "Id", "Name"),
                ExpenseCategories = new SelectList(await _context.Categories
                    .Where(c => c.ApplicationUserId == userId && c.Type == CategoryType.Expense)
                    .ToListAsync(), "Id", "Name"),
                IncomeCategories = new SelectList(await _context.Categories
                    .Where(c => c.ApplicationUserId == userId && c.Type == CategoryType.Income)
                    .ToListAsync(), "Id", "Name")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                Transaction transaction = model.TransactionType == "Income" ? new Income() : new Expense();

                transaction.Amount = model.Amount;
                transaction.AccountId = model.AccountId;
                transaction.CategoryId = model.CategoryId;
                transaction.Date = model.Date;
                transaction.Comment = model.Comment;

                if (!string.IsNullOrWhiteSpace(model.Tags))
                {
                    await ProcessTags(model.Tags, transaction, userId);
                }

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            // If model is invalid, re-populate the dropdowns before returning the view
            model.Accounts = new SelectList(await _context.Accounts
                .Where(a => a.ApplicationUserId == userId && a.IsActive)
                .ToListAsync(), "Id", "Name", model.AccountId);
            model.ExpenseCategories = new SelectList(await _context.Categories
                .Where(c => c.ApplicationUserId == userId && c.Type == CategoryType.Expense)
                .ToListAsync(), "Id", "Name", model.CategoryId);
            model.IncomeCategories = new SelectList(await _context.Categories
                .Where(c => c.ApplicationUserId == userId && c.Type == CategoryType.Income)
                .ToListAsync(), "Id", "Name", model.CategoryId);

            return View(model);
        }

        private async Task ProcessTags(string tags, Transaction transaction, string userId)
        {
            var tagNames = tags.Split(',').Select(t => t.Trim().ToLower()).Where(t => !string.IsNullOrEmpty(t));

            foreach (var tagName in tagNames)
            {
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName && t.ApplicationUserId == userId);

                if (tag == null)
                {
                    tag = new Tag { Name = tagName, ApplicationUserId = userId };
                    _context.Tags.Add(tag);
                }

                transaction.TransactionTags.Add(new TransactionTag { Tag = tag });
            }
        }
    }
}