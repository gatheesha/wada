namespace wada.Models
{
    public class MilestoneModel
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime Deadline { get; set; }
        public int ProjectId { get; set; }
    }
}