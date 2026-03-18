namespace BussinessObjects.DTOs.admin.financial
{
    public class FinancialStatsDTO
    {
        public decimal TotalRevenue { get; set; }
        public double RevenueGrowth { get; set; }
        public double ExpensesGrowth { get; set; }
        public double ProfitGrowth { get; set; }
        public decimal OperationalExpenses { get; set; } // Giả lập dựa trên % hoặc bảng chi phí nếu có
        public decimal NetProfit { get; set; }
        public decimal PrevRevenue { get; set; }
        public decimal PrevExpenses { get; set; }
        public decimal PrevProfit { get; set; }

    }
}
