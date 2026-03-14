using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class AppointmentApiService : IAppointmentApiService
    {
        private readonly IApiClient _apiClient;

        public AppointmentApiService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<AppointmentDTO>?> CreateAppointmentAsync(CreateAppointmentRequestDTO request)
        {
            return await _apiClient.PostAsync<AppointmentDTO>("/api/Appointment", request);
        }

        public async Task<ApiResponse<List<AppointmentDTO>>?> GetMyAppointmentsAsync()
        {
            return await _apiClient.GetAsync<List<AppointmentDTO>>("/api/Appointment/my");
        }

        public async Task<ApiResponse<AppointmentDTO>?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _apiClient.GetAsync<AppointmentDTO>($"/api/Appointment/{appointmentId}");
        }

        public async Task<ApiResponse<AppointmentDTO>?> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequestDTO request)
        {
            return await _apiClient.PutAsync<AppointmentDTO>($"/api/Appointment/{appointmentId}", request);
        }

        public async Task<ApiResponse<AppointmentDTO>?> CancelAppointmentAsync(int appointmentId, CancelAppointmentRequestDTO request)
        {
            return await _apiClient.PutAsync<AppointmentDTO>($"/api/Appointment/{appointmentId}/cancel", request);
        }
    }
}
