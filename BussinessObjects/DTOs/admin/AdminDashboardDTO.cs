namespace BussinessObjects.DTOs.admin
{
    public class AdminDashboardDto
    {
        public decimal MonthlyRevenue { get; set; }
        public double RevenueGrowth { get; set; }
        public int TotalAppointments { get; set; }
        public double AppointmentGrowth { get; set; }
        public int NewPatients { get; set; }
        public double PatientGrowth { get; set; }
        public int ActiveDoctors { get; set; }
        public double DoctorGrowth { get; set; }
    }
}
