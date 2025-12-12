using FinanceManager.Data;
using FinanceManager.Models;
using FinanceManager.Models.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceManager.Areas.Identity.Pages.Account.Manage
{
    public class RegularPaymentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Enums> _enumsLocalizer;
        private readonly IStringLocalizer<RegularPaymentsPage> _pageLocalizer;

        public class PaymentViewModel
        {
            public RegularPayment Payment { get; set; }
            public string FormattedFrequency { get; set; }
        }

        public RegularPaymentsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Enums> enumsLocalizer,
            IStringLocalizer<RegularPaymentsPage> pageLocalizer)
        {
            _context = context;
            _userManager = userManager;
            _enumsLocalizer = enumsLocalizer;
            _pageLocalizer = pageLocalizer;
        }

        public List<PaymentViewModel> Payments { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            var paymentsFromDb = await _context.RegularPayments
                .Where(rp => rp.ApplicationUserId == userId)
                .Include(rp => rp.Account)
                .Include(rp => rp.Category)
                .OrderBy(rp => rp.NextRunDate)
                .ToListAsync();

            Payments = paymentsFromDb.Select(p => new PaymentViewModel
            {
                Payment = p,
                FormattedFrequency = _pageLocalizer.GetString("Every", p.Interval, _enumsLocalizer[$"FrequencyUnit_{p.FrequencyUnit.ToString()}"])
            }).ToList();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var payment = await _context.RegularPayments.FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == userId);

            if (payment != null)
            {
                payment.IsActive = !payment.IsActive;
                await _context.SaveChangesAsync();
                StatusMessage = "Payment status has been updated.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var payment = await _context.RegularPayments.FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == userId);

            if (payment != null)
            {
                if (payment.StartDate <= System.DateTime.Today)
                {
                    StatusMessage = "Error: Cannot delete a regular payment that has already started. You can cancel it instead.";
                    return RedirectToPage();
                }

                _context.RegularPayments.Remove(payment);
                await _context.SaveChangesAsync();
                StatusMessage = "Payment has been permanently deleted.";
            }
            return RedirectToPage();
        }
    }
}