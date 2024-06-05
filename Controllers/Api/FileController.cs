using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models;
using System.IO;

namespace Project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Upload")]
        public IActionResult UploadFile(IFormFile file, int folderId)
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
                _context.SaveChanges();
                return Ok(newFile);
            }
            return BadRequest("Invalid file");
        }
    }
}
