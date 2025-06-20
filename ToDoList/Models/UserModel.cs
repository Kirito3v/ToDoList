using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoList.Models
{
    public class UserModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Salt { get; set; }

        [ForeignKey("UserData")]
        public Guid UserId { get; set; }
        public UserData? UserData { get; set; }

        public DateTime LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
    }
}
