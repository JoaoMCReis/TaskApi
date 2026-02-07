namespace TaskApi.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = "member"; // admin | manager | member
    }
}
