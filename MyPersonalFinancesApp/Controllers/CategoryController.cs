using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManager.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category/Create
        public IActionResult Create(string type)
        {
            var viewModel = new CategoryCreateViewModel
            {
                Type = type == "Income" ? CategoryType.Income : CategoryType.Expense
            };
            return View(viewModel);
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var category = new Category
                {
                    Name = model.Name,
                    Color = model.Color,
                    Type = model.Type,
                    ApplicationUserId = userId
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // Redirect back to the transaction create page for a seamless flow
                return RedirectToAction("Create", "Transaction", new { type = model.Type.ToString() });
            }
            return View(model);
        }
    }
}