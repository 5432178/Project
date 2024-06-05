using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using SelectPdf;
using iTextSharp.text.pdf;
namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(userIdStr);
            var folders = _context.Folders.Where(f => f.CreatedBy == userId && f.IsActive).ToList();
            var files = _context.Files.Where(f => f.CreatedBy == userId && f.IsActive).ToList();

            var viewModel = new FolderFileViewModel
            {
                Folders = folders ?? new List<Folder>(),
                Files = files ?? new List<AppFiles>(),
                SearchResults = new List<AppFiles>() // Initialize search results
            };

            return View(viewModel);
        }

        public IActionResult Login(UserLoginViewModel model)
        {
            // Assume user authentication is successful and user object is retrieved from the database
            var user = _context.Users.SingleOrDefault(u => u.Login == model.Login && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name); // Set the user's name in session
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder(string folderName)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdStr);
            var newFolder = new Folder
            {
                Name = folderName,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _context.Folders.Add(newFolder);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int folderId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdStr);
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExt = Path.GetExtension(fileName).Substring(1);
                var fileSizeInKB = (int)(file.Length / 1024);

                var newFile = new AppFiles
                {
                    Name = fileName,
                    ParentFolderId = folderId,
                    FileExt = fileExt,
                    FileSizeInKB = fileSizeInKB,
                    CreatedBy = userId,
                    UploadedOn = DateTime.Now,
                    IsActive = true
                };

                _context.Files.Add(newFile);
                await _context.SaveChangesAsync();

                // Ensure the directory exists
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Save the file to the server
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return RedirectToAction("Index");
            }
            return BadRequest("Invalid file");
        }

        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.Folders
        .Include(f => f.Files)
        .Include(f => f.SubFolders)
        .FirstOrDefaultAsync(f => f.Id == id);
            if (folder == null)
            {
                return NotFound();
            }

            foreach (var file in folder.Files)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", file.Name);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");


        }

        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            // Mark the file as inactive
            file.IsActive = false;
            _context.Files.Update(file);
            await _context.SaveChangesAsync();

            // Delete the file from the server
            var filePath = Path.Combine("wwwroot/files", file.Name);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult FolderContent(int folderId)
        {
            // Retrieve folder information and files for the specified folderId
            var folder = _context.Folders.FirstOrDefault(f => f.Id == folderId);
            var folders = _context.Folders.Where(f => f.ParentFolderId == folderId).ToList();
            var files = _context.Files.Where(f => f.ParentFolderId == folderId).ToList();

            // Construct the current folder path for navigation
            var currentFolderPath = BuildFolderPath(folder);

            var viewModel = new FolderContentViewModel
            {
                Folders = folders,
                Files = files,
                CurrentFolderPath = currentFolderPath
            };

            ViewBag.CurrentFolderId = folderId;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubFolder(string subFolderName, int parentFolderId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdStr);
            var subFolder = new Folder
            {
                Name = subFolderName,
                ParentFolderId = parentFolderId,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _context.Folders.Add(subFolder);
            await _context.SaveChangesAsync();

            return RedirectToAction("FolderContent", new { folderId = parentFolderId });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileToSubFolder(IFormFile file, int parentFolderId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdStr);
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExt = Path.GetExtension(fileName).Substring(1);
                var fileSizeInKB = (int)(file.Length / 1024);

                var newFile = new AppFiles
                {
                    Name = fileName,
                    ParentFolderId = parentFolderId,
                    FileExt = fileExt,
                    FileSizeInKB = fileSizeInKB,
                    CreatedBy = userId,
                    UploadedOn = DateTime.Now,
                    IsActive = true
                };

                _context.Files.Add(newFile);
                await _context.SaveChangesAsync();

                // Ensure the directory exists
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Save the file to the server
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return RedirectToAction("FolderContent", new { folderId = parentFolderId });
            }
            return BadRequest("Invalid file");
        }

        private string BuildFolderPath(Folder folder)
        {
            // Implement logic to construct the folder path (e.g., based on parent folders)
            // Example: "Root > Documents > Folder1"
            if (folder == null)
            {
                return "Root";
            }

            var path = folder.Name;
            var parent = _context.Folders.FirstOrDefault(f => f.Id == folder.ParentFolderId);

            while (parent != null)
            {
                path = parent.Name + " > " + path;
                parent = _context.Folders.FirstOrDefault(f => f.Id == parent.ParentFolderId);
            }

            return "Root > " + path;
        }

        public IActionResult DownloadFile(int id)
        {
            var file = _context.Files.FirstOrDefault(f => f.Id == id);
            if (file == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine("wwwroot/files", file.Name);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream";
            return File(fileBytes, contentType, file.Name);
        }
        public IActionResult DownloadMetaAsPdf()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(userIdStr); // Corrected this line

            HtmlToPdf converter = new SelectPdf.HtmlToPdf();

            try
            {
                var folders = _context.Folders.Where(f => f.CreatedBy == userId && f.IsActive).ToList();
                var files = _context.Files.Where(f => f.CreatedBy == userId && f.IsActive).ToList();

                var htmlContent = new StringBuilder();
                htmlContent.Append("<html>");
                htmlContent.Append("<head><title>Folder Meta Information</title></head>");
                htmlContent.Append("<body>");
                htmlContent.Append("<h1>Folder Meta Information</h1>");
                htmlContent.Append("<ul>");

                foreach (var folder in folders)
                {
                    htmlContent.Append($"<li>Folder: {folder.Name} (FolderId: {folder.Id})</li>");
                }
                htmlContent.Append("<body>");
                htmlContent.Append("<h1>File Meta Information</h1>");
                htmlContent.Append("<ul>");
                foreach (var file in files)
                {
                    htmlContent.Append($"<li>File: {file.Name} ({file.FileExt}, {file.FileSizeInKB} KB, parentFolder {file.ParentFolderId})</li>");
                }

                htmlContent.Append("</ul>");
                htmlContent.Append("</body>");
                htmlContent.Append("</html>");

                SelectPdf.PdfDocument doc = converter.ConvertHtmlString(htmlContent.ToString(), "http://localhost"); // Corrected the URL

                // Get bytes
                var bytes = doc.Save();

                doc.Close();

                return File(bytes, "application/pdf", "MetaInformation.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating the PDF: {ex.Message}");
                return Content($"An error occurred while generating the PDF: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult Search(string searchTerm)
        {

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(userIdStr);

            var searchResults = _context.Files
                .Where(f => f.CreatedBy == userId && f.IsActive && f.Name.Contains(searchTerm))
                .ToList();

            var folders = _context.Folders.Where(f => f.CreatedBy == userId && f.IsActive).ToList();
            var files = _context.Files.Where(f => f.CreatedBy == userId && f.IsActive).ToList();

            var viewModel = new FolderFileViewModel
            {
                Folders = folders ?? new List<Folder>(),
                Files = files ?? new List<AppFiles>(),
                SearchResults = searchResults
            };

            return View("Index", viewModel); // Return the same view with search results
        }

        // ... other methods ...
    

}
}
