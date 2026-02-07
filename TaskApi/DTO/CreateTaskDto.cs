namespace TaskApi.DTO
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid? AssignedToId { get; set; }
    }
}
