namespace TaskApi.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public Guid AssignedToId { get; set; } // opcional
        public User? AssignedTo { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
