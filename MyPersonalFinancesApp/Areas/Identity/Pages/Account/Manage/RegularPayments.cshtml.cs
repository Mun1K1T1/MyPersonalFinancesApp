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
    public class RegularPaymentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegularPaymentsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<RegularPayment> Payments { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            Payments = await _context.RegularPayments
                .Where(rp => rp.ApplicationUserId == userId)
                .Include(rp => rp.Account)
                .Include(rp => rp.Category)
                .OrderBy(rp => rp.NextRunDate)
                .ToListAsync();
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
                // Rule: Cannot delete a payment that has already started.
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