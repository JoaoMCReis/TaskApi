namespace TaskApi.DTO
{
    public class UpdateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public Guid? AssignedToId { get; set; } // ✅ ADICIONAR ISTO
    }
}
