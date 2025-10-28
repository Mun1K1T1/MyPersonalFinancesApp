using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace FinanceManager.Controllers
{
    [Authorize]
    public class AccountSetupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountSetupController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var account = new Account
                {
                    Name = model.Name,
                    Currency = model.Currency,
                    InitialBalance = model.InitialBalance,
                    ApplicationUserId = userId,
                    IsActive = true
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                // If the initial balance is greater than 0, create a corresponding income transaction.
                if (model.InitialBalance > 0)
                {
                    var initialBalanceCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && c.Name == "Initial Balance");

                    if (initialBalanceCategory != null)
                    {
                        var initialIncome = new Income
                        {
                            Amount = model.InitialBalance,
                            Date = DateTime.Now,
                            Comment = "Initial account balance.",
                            AccountId = account.Id,
                            CategoryId = initialBalanceCategory.Id
                        };
                        _context.Transactions.Add(initialIncome);
                        await _context.SaveChangesAsync(); // Save the new transaction
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}