using FinanceManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManager.Filters
{
    public class EnsureUserHasAccountFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;

        public EnsureUserHasAccountFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                await next();
                return;
            }

            var isAccountSetupController = context.Controller.GetType().Name == "AccountSetupController";
            if (isAccountSetupController)
            {
                await next();
                return;
            }
            var hasAccounts = await _context.Accounts.AnyAsync(a => a.ApplicationUserId == userId && a.IsActive);

            if (!hasAccounts)
            {
                context.Result = new RedirectToActionResult("Create", "AccountSetup", null);
                return;
            }

            await next();
        }
    }
}