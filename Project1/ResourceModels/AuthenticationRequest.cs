using System.ComponentModel.DataAnnotations;

namespace Project1.ResourceModels
{
    public class AuthenticationRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
