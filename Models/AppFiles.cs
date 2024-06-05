using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    [Table("Files")]
    public class AppFiles
    {
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string Name { get; set; }
        public int ParentFolderId { get; set; }
        [MaxLength(10)]
        public string FileExt { get; set; }
        public int FileSizeInKB { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("ParentFolderId")]
        public Folder ParentFolder { get; set; }

        // Placeholder property for thumbnail URL
        public string ThumbnailUrl => GenerateThumbnailUrl();

        private string GenerateThumbnailUrl()
        {
            // Implement logic to generate the thumbnail URL based on file type
            // Example: return "/thumbnails/default.png"; for files without specific thumbnails
            return "/thumbnails/default.png"; // Replace with your logic

        }
    }
}
