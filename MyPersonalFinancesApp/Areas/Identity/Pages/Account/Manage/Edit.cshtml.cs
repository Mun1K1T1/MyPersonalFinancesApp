using FinanceManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FinanceManager.Models;

namespace FinanceManager.Areas.Identity.Pages.Account.Manage
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Color { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            // Validation: Check if another category with the same name exists for this user
            var nameExists = await _context.Categories.AnyAsync(c => c.ApplicationUserId == userId && c.Name == Input.Name && c.Id != Input.Id);
            if (nameExists)
            {
                ModelState.AddModelError("Input.Name", "A category with this name already exists.");
                return Page();
            }

            var categoryToUpdate = await _context.Categories.FindAsync(Input.Id);
            if (categoryToUpdate == null)
            {
                return NotFound();
            }

            categoryToUpdate.Name = Input.Name;
            categoryToUpdate.Color = Input.Color;

            await _context.SaveChangesAsync();
            StatusMessage = "Category has been updated.";
            return RedirectToPage("./Categories");
        }
    }
}