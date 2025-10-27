using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManager.Models
{
    public enum Currency { USD, EUR, UAH }

    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Currency Currency { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InitialBalance { get; set; }

        public bool IsActive { get; set; } = true;

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}