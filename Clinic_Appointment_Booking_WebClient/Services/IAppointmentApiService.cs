using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IAppointmentApiService
    {
        Task<ApiResponse<AppointmentDTO>?> CreateAppointmentAsync(CreateAppointmentRequestDTO request);
        Task<ApiResponse<List<AppointmentDTO>>?> GetMyAppointmentsAsync();
        Task<ApiResponse<AppointmentDTO>?> GetAppointmentByIdAsync(int appointmentId);
        Task<ApiResponse<AppointmentDTO>?> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequestDTO request);
        Task<ApiResponse<AppointmentDTO>?> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDTO request);
    }
}
