using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManager.Models
{
    public enum FrequencyUnit
    {
        Day,
        Week,
        Month,
        Year
    }

    public class RegularPayment
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        public CategoryType Type { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public FrequencyUnit FrequencyUnit { get; set; }

        [Required, Range(1, 100)]
        public int Interval { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public TimeSpan TimeOfDay { get; set; }

        public DateTime NextRunDate { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}