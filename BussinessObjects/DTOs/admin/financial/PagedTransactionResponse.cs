namespace BussinessObjects.DTOs.admin.financial
{
    public class PagedTransactionResponse
    {
        public IEnumerable<TransactionResponseDTO> Data { get; set; } = new List<TransactionResponseDTO>();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
