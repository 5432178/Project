namespace Project.Models
{
    public class FolderFileViewModel
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();
        public List<AppFiles> Files { get; set; } = new List<AppFiles>();
        public List<AppFiles> SearchResults { get; set; } = new List<AppFiles>();
    }
}
