namespace MyApp.Api.DTOs.Sites
{
    public class CreateSiteDto
    {
        public string SiteName { get; set; } = string.Empty;

        public string? DomainName { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string ThemeKey { get; set; } = "theme-1";

        public bool IsActive { get; set; } = true;

        public int? WebsiteTemplateId { get; set; }
        public int? WebsiteThemeId { get; set; }
    }
}