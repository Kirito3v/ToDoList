using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoList.Models
{
    public class ToDoItemModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;

        [ForeignKey("UserProfile")]
        public Guid UserId { get; set; }
        public UserData? UserData { get; set; }
    }
}
