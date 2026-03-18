namespace BussinessObjects.DTOs.admin.financial
{
    public class FinancialAnalyticsDTO
    {
        // Dữ liệu cho biểu đồ đường (6 tháng)
        public List<ChartDataPoint> RevenueTrends { get; set; }

        // Dữ liệu cho các thanh Progress chi phí
        public List<ExpenseBreakdownDTO> ExpenseBreakdown { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } // JAN, FEB...
        public decimal Value { get; set; }
    }

    public class ExpenseBreakdownDTO
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }
}
