using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FinanceManager.Models
{
    public enum TimePeriod
    {
        Day,
        Week,
        Month,
        Year,
        AllTime
    }

    // Class to hold data charts
    public class ChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
        public List<string> Colors { get; set; } = new List<string>();
    }

    public class DashboardViewModel
    {
        // For the account filter dropdown
        public int? SelectedAccountId { get; set; }
        public SelectList? Accounts { get; set; }
        public SelectList? TimePeriods { get; set; }
        public decimal CurrentAmount { get; set; }
        public string SelectedTimePeriod { get; set; }
        // Lists for the 20 recent transactions
        public List<Income> RecentIncomes { get; set; } = new List<Income>();
        public List<Expense> RecentExpenses { get; set; } = new List<Expense>();

        public int TotalIncomeCountForPeriod { get; set; }
        public int TotalExpenseCountForPeriod { get; set; }

        // Data for the pie charts
        public ChartData IncomeChartData { get; set; } = new ChartData();
        public ChartData ExpenseChartData { get; set; } = new ChartData();
    }
}