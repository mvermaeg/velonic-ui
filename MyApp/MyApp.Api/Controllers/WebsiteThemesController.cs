using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebsiteThemesController : ControllerBase
    {
        private readonly MyAppDbContext _context;

        public WebsiteThemesController(MyAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetThemes()
        {
            var data = await _context.WebsiteThemes
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ThemeName,
                    x.ThemeKey,
                    x.PrimaryColor,
                    x.SecondaryColor,
                    x.FontFamily,
                    x.LogoUrl,
                    x.CustomCss,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}