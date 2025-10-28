using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models // The namespace stays consistent with your other models
{
    public class TransactionCreateViewModel
    {
        [Required]
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; } // "Expense" or "Income"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Account")]
        public int AccountId { get; set; }
        public SelectList? Accounts { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public SelectList? ExpenseCategories { get; set; }
        public SelectList? IncomeCategories { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Tags (comma-separated)")]
        public string? Tags { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        // Future use: public IFormFileCollection Attachments { get; set; }

        // Fields for Regular Payments
        [Display(Name = "Make it regular")]
        public bool IsRegular { get; set; }

        public string? PaymentName { get; set; }
        public Frequency Frequency { get; set; }
        public DateTime? EndDate { get; set; }
    }
}