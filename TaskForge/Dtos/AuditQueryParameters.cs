namespace TaskForge.Dtos
{
    public class AuditQueryParameters
    {
        public int? AfterId { get; set; }
        public int Limit { get; set; } = 50;
    }
}