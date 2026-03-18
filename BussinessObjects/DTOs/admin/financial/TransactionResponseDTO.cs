namespace BussinessObjects.DTOs.admin.financial
{
    public class TransactionResponseDTO
    {
        public string TransactionId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty; // Tên bệnh nhân hoặc NCC
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = "Income"; // Income hoặc Expense
    }
}
