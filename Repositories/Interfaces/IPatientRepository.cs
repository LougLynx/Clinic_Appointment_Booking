namespace Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<(IEnumerable<dynamic> Data, int TotalCount)> GetPatientsByDoctorIdAsync(
            int doctorId,
            int pageNumber,
            int pageSize,
            string search = null,
            string status = null);
    }
}
