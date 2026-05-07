namespace MyApp.Api.DTOs.Sites
{
    public class UpdateSiteTemplateThemeDto
    {
        public int? WebsiteTemplateId { get; set; }
        public int? WebsiteThemeId { get; set; }
        public string ThemeKey { get; set; } = "theme-1";
    }
}