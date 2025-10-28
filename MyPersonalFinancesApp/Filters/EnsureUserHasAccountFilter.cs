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
            // Get the user's ID from the claims
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If the user is not logged in, do nothing. The [Authorize] attribute will handle them.
            if (string.IsNullOrEmpty(userId))
            {
                await next();
                return;
            }

            // Check if we are already on the AccountSetup controller to prevent infinite redirects
            var isAccountSetupController = context.Controller.GetType().Name == "AccountSetupController";
            if (isAccountSetupController)
            {
                await next();
                return;
            }

            // Check if the user has any active accounts in the database
            var hasAccounts = await _context.Accounts.AnyAsync(a => a.ApplicationUserId == userId && a.IsActive);

            if (!hasAccounts)
            {
                // If no accounts, redirect them to the account creation page
                context.Result = new RedirectToActionResult("Create", "AccountSetup", null);
                return;
            }

            // If they have accounts, proceed with the original request
            await next();
        }
    }
}