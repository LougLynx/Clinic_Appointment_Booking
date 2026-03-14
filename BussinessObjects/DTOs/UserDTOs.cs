namespace BussinessObjects.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
