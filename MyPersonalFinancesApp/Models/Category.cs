using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models
{
    public enum CategoryType
    {
        Income,
        Expense
    }

    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public CategoryType Type { get; set; }

        [Required]
        [StringLength(7)] 
        public string Color { get; set; } = "#808080"; // Default to grey

        [Required]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}