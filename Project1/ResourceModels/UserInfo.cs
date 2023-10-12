using System.ComponentModel.DataAnnotations;

namespace Project1.ResourceModels
{
    public class UserInfo
    {
        public string? FirstName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
    }
}
