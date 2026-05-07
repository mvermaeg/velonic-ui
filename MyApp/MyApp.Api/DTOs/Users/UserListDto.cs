namespace MyApp.Api.DTOs.Users
{
    public class UserListDto
    {
        public string Id { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public bool IsActive { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}