using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models;

namespace Project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FolderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public IActionResult CreateFolder(string folderName)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdStr);
            var folder = new Folder
            {
                Name = folderName,
                CreatedBy = userId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };
            _context.Folders.Add(folder);
            _context.SaveChanges();
            return Ok(folder);
        }

        [HttpGet]
        public IActionResult GetFolders()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdStr);
            var folders = _context.Folders.Where(f => f.CreatedBy == userId && f.ParentFolderId == null).ToList();
            return Ok(folders);
        }
    }
}
