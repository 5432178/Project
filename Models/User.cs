using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("User")]
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string Name { get; set; }
        [Required, MaxLength(20)]
        public string Login { get; set; }
        [Required, MaxLength(20)]
        public string Password { get; set; }
        [Required, MaxLength(20)]
        public string Email { get; set; }
    }
}
