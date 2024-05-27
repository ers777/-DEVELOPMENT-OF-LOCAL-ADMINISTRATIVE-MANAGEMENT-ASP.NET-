using System.ComponentModel.DataAnnotations;

namespace Nurast.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string FIO { get; set; }
        public string PhoneNumber { get; set; }
        public string Date { get; set; }
        public string Famele { get; set; }
        public string Сategory { get; set; }
    }
}
