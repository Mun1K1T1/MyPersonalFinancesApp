using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models
{
    public class CategoryCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Category Color")]
        public string Color { get; set; } = "#FF6384";

        [Required]
        public CategoryType Type { get; set; }
    }
}