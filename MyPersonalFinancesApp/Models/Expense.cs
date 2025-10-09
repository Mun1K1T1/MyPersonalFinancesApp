using System.ComponentModel.DataAnnotations;

namespace MyPersonalFinancesApp.Models
{
    public class Expense
    {
        public int Id { get; set;}
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount has to be higher than zero")]
        public double Amount { get; set; }
        [Required]
        public int BalanceId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Tag { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
