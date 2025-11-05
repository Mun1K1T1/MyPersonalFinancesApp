using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceManager.Areas.Identity.Pages.Account.Manage
{
    public class CreateRegularPaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateRegularPaymentModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Accounts { get; set; }
        public SelectList ExpenseCategories { get; set; }
        public SelectList IncomeCategories { get; set; }

        public class InputModel
        {
            [Required, StringLength(100)]
            public string Name { get; set; }

            [Required]
            public CategoryType Type { get; set; }

            [Required, Range(0.01, double.MaxValue)]
            public decimal Amount { get; set; }

            [Required, Display(Name = "Account")]
            public int AccountId { get; set; }

            [Required, Display(Name = "Category")]
            public int CategoryId { get; set; }

            [Required, Display(Name = "Frequency")]
            public FrequencyUnit FrequencyUnit { get; set; }

            [Required, Range(1, 100)]
            [Display(Name = "Every")]
            public int Interval { get; set; } = 1;

            [Required, Display(Name = "Start Date")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; }

            [Display(Name = "End Date (Optional)")]
            [DataType(DataType.Date)]
            public DateTime? EndDate { get; set; }

            [Required, Display(Name = "Time of Day")]
            [DataType(DataType.Time)]
            public TimeSpan TimeOfDay { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadSelectListsAsync();
            Input = new InputModel
            {
                StartDate = DateTime.Today,
                TimeOfDay = DateTime.Now.TimeOfDay
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            var paymentExists = await _context.RegularPayments.AnyAsync(p =>
                p.ApplicationUserId == userId &&
                p.Name == Input.Name &&
                p.FrequencyUnit == Input.FrequencyUnit &&
                p.Interval == Input.Interval);

            if (paymentExists)
            {
                ModelState.AddModelError("Input.Name", "A regular payment with the same name and frequency already exists.");
                await LoadSelectListsAsync();
                return Page();
            }

            var newPayment = new RegularPayment
            {
                Name = Input.Name,
                Type = Input.Type,
                Amount = Input.Amount,
                AccountId = Input.AccountId,
                CategoryId = Input.CategoryId,
                FrequencyUnit = Input.FrequencyUnit,
                Interval = Input.Interval,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                TimeOfDay = Input.TimeOfDay,
                NextRunDate = Input.StartDate.Date + Input.TimeOfDay,
                ApplicationUserId = userId,
                IsActive = true
            };

            _context.RegularPayments.Add(newPayment);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "New regular payment created successfully.";
            return RedirectToPage("./RegularPayments");
        }

        private async Task LoadSelectListsAsync()
        {
            var userId = _userManager.GetUserId(User);
            Accounts = new SelectList(await _context.Accounts.Where(a => a.ApplicationUserId == userId).ToListAsync(), "Id", "Name");
            ExpenseCategories = new SelectList(await _context.Categories.Where(c => c.ApplicationUserId == userId && c.Type == CategoryType.Expense).ToListAsync(), "Id", "Name");
            IncomeCategories = new SelectList(await _context.Categories.Where(c => c.ApplicationUserId == userId && c.Type == CategoryType.Income).ToListAsync(), "Id", "Name");
        }
    }
}