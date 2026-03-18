namespace BussinessObjects.DTOs.admin
{
    public class PagedMedicalStaffResponse
    {
        public IEnumerable<MedicalStaffResponseDTO> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
