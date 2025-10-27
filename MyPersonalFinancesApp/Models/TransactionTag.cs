namespace FinanceManager.Models
{
    public class TransactionTag
    {
        public int TransactionId { get; set; }
        public virtual Transaction Transaction { get; set; }

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}