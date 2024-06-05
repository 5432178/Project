using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("Folder")]
    public class Folder
    {
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public ICollection<AppFiles> Files { get; set; } = new List<AppFiles>();


        // Add this property to manage subfolders
        public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();

        // Navigation property for parent folder
        [ForeignKey("ParentFolderId")]
        public Folder ParentFolder { get; set; }
    }
}
