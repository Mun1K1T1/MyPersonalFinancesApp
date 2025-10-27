using System.ComponentModel.DataAnnotations;
using FinanceManager.Models;

namespace FinanceManager.Models
{
    public class CreateAccountViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Required]
        public Currency Currency { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Initial Balance")]
        public decimal InitialBalance { get; set; }
    }
}