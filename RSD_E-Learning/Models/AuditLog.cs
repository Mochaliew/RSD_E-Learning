using System;

namespace RSD_E_Learning.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

    }
}
