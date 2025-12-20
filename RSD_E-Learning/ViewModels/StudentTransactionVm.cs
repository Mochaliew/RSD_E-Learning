namespace RSD_E_Learning.ViewModels
{
    public class StudentTransactionVm
    {
        public string CourseTitle { get; set; } = "";
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public DateTime TransactionDate { get; set; }
    }
}
