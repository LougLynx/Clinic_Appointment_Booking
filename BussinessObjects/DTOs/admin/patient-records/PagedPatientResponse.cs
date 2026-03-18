namespace BussinessObjects.DTOs.admin.patient_records
{
    public class PagedPatientResponse
    {
        public IEnumerable<PatientRecordResponseDto> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
