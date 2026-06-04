using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SiteMediaFilesController : ControllerBase
    {
        private readonly MyAppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SiteMediaFilesController(MyAppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet("by-site/{siteId}")]
        public async Task<IActionResult> GetBySite(long siteId)
        {
            var data = await _db.SiteMediaFiles
                .Where(x => x.SiteId == siteId && x.IsActive)
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    x.FileUrl,
                    x.FileName,
                    x.FileType,
                    x.MimeType,
                    x.FileSizeBytes,
                    x.AltText,
                    x.Caption,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadSiteMediaDto model)
        {
            if (model.SiteId <= 0)
                return BadRequest(new { message = "Invalid site." });

            if (model.File == null || model.File.Length == 0)
                return BadRequest(new { message = "File is required." });

            var allowed = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp",
                "image/gif",
                "video/mp4",
                "application/pdf"
            };

            if (!allowed.Contains(model.File.ContentType))
                return BadRequest(new { message = "Unsupported file type." });

            var webRoot = _env.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }

            var uploadRoot = Path.Combine(webRoot, "uploads", "sites", model.SiteId.ToString());
            if (!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            var extension = Path.GetExtension(model.File.FileName);
            var safeName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadRoot, safeName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/sites/{model.SiteId}/{safeName}";

            var entity = new SiteMediaFile
            {
                SiteId = model.SiteId,
                FileUrl = fileUrl,
                FileName = model.File.FileName,
                FileType = model.File.ContentType.StartsWith("image/")
                    ? "image"
                    : model.File.ContentType.StartsWith("video/")
                        ? "video"
                        : "document",
                MimeType = model.File.ContentType,
                FileSizeBytes = model.File.Length,
                AltText = model.AltText,
                Caption = model.Caption,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            _db.SiteMediaFiles.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "File uploaded successfully.",
                entity.Id,
                entity.FileUrl
            });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
        {
            var entity = await _db.SiteMediaFiles.FindAsync(id);

            if (entity == null)
                return NotFound(new { message = "Media file not found." });

            entity.IsActive = isActive;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Media status updated successfully." });
        }
    }

    public class UploadSiteMediaDto
    {
        public long SiteId { get; set; }
        public IFormFile? File { get; set; }
        public string? AltText { get; set; }
        public string? Caption { get; set; }
    }
}