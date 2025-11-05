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
    public class EditRegularPaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditRegularPaymentModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public SelectList Accounts { get; set; }
        public SelectList ExpenseCategories { get; set; }
        public SelectList IncomeCategories { get; set; }

        public class InputModel
        {
            [Required]
            public int Id { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var payment = await _context.RegularPayments.FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == userId);

            if (payment == null)
            {
                return NotFound();
            }

            if (payment.StartDate < DateTime.Today)
            {
                StatusMessage = "Error: Cannot edit a regular payment that has already started.";
                return RedirectToPage("./RegularPayments");
            }

            await LoadSelectListsAsync();

            Input = new InputModel
            {
                Id = payment.Id,
                Name = payment.Name,
                Type = payment.Type,
                Amount = payment.Amount,
                AccountId = payment.AccountId,
                CategoryId = payment.CategoryId,
                FrequencyUnit = payment.FrequencyUnit,
                Interval = payment.Interval,
                StartDate = payment.StartDate,
                EndDate = payment.EndDate,
                TimeOfDay = payment.TimeOfDay
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
                p.Id != Input.Id &&
                p.Name == Input.Name &&
                p.FrequencyUnit == Input.FrequencyUnit &&
                p.Interval == Input.Interval);

            if (paymentExists)
            {
                ModelState.AddModelError("Input.Name", "Another regular payment with the same name and frequency already exists.");
                await LoadSelectListsAsync();
                return Page();
            }

            var paymentToUpdate = await _context.RegularPayments.FirstOrDefaultAsync(p => p.Id == Input.Id && p.ApplicationUserId == userId);
            if (paymentToUpdate == null)
            {
                return NotFound();
            }

            paymentToUpdate.Name = Input.Name;
            paymentToUpdate.Type = Input.Type;
            paymentToUpdate.Amount = Input.Amount;
            paymentToUpdate.AccountId = Input.AccountId;
            paymentToUpdate.CategoryId = Input.CategoryId;
            paymentToUpdate.FrequencyUnit = Input.FrequencyUnit;
            paymentToUpdate.Interval = Input.Interval;
            paymentToUpdate.StartDate = Input.StartDate;
            paymentToUpdate.EndDate = Input.EndDate;
            paymentToUpdate.TimeOfDay = Input.TimeOfDay;

            paymentToUpdate.NextRunDate = Input.StartDate.Date + Input.TimeOfDay;

            await _context.SaveChangesAsync();
            StatusMessage = "Regular payment has been updated.";
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