namespace Project.Models
{
    public class FolderContentViewModel
    {
        public List<Folder> Folders { get; set; }
        public List<AppFiles> Files { get; set; }
        public string CurrentFolderPath { get; set; }
    }
}
