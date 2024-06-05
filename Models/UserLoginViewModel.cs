using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class UserLoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
