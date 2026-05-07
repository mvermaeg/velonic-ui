using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WebsiteTemplatesController : ControllerBase
    {
        private readonly MyAppDbContext _context;

        public WebsiteTemplatesController(MyAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            var data = await _context.WebsiteTemplates
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.TemplateName,
                    x.TemplateKey,
                    x.PreviewImage,
                    x.SourceFolder,
                    x.BuildFolder,
                    x.Version,
                    x.Description,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}