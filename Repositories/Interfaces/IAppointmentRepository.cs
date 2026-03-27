using BussinessObjects.Models;

namespace Repositories.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<Appointment?> GetByIdWithDetailsAsync(int appointmentId);
        Task<bool> ExistsSlotAsync(int doctorId, DateTime date, TimeSpan time);
        Task<IEnumerable<Appointment>> GetBusySlotsAsync(int doctorId, DateTime startDate, DateTime endDate);
    }
}
