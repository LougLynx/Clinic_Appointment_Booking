namespace BussinessObjects.DTOs
{
    public class DoctorDTO
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string? Qualifications { get; set; }
        public int YearsOfExperience { get; set; }
        public string? Location { get; set; }
        public string? Languages { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class DoctorDetailDTO : DoctorDTO
    {
        public List<DoctorScheduleDTO> Schedules { get; set; } = new();
        public List<string> Specializations { get; set; } = new();
        public new List<string> Languages { get; set; } = new();
        public List<TimeSlotDTO> AvailableTimeSlots { get; set; } = new();
        public List<TimeSlotDTO> BusySlots { get; set; } = new();
        public string? Education { get; set; }
    }

    public class TimeSlotDTO
    {
        public int TimeSlotId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class DoctorScheduleDTO
    {
        public int ScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? SpecificDate { get; set; }
    }

    public class DoctorSearchRequestDTO
    {
        public string? SearchTerm { get; set; }
        public int? SpecialtyId { get; set; }
        public string? Gender { get; set; }
        public bool? AvailableToday { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DoctorSearchResponseDTO
    {
        public List<DoctorDTO> Doctors { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int CurrentPage => PageNumber;
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
