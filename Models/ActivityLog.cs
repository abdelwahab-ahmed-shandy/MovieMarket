namespace MovieMart.Models
{
    public class ActivityLog
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Action { get; set; }

        [StringLength(500)]
        public string Details { get; set; }

        public ActivityStatus Status { get; set; } = ActivityStatus.Success;

        public virtual ApplicationUser User { get; set; }
    }
}
