using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ErrorLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required string Level { get; set; }  // e.g. "Error", "Warning"
        public required string Message { get; set; }  // the human-readable message
        public string? ExceptionType { get; set; }  // ex.GetType().FullName
        public string? StackTrace { get; set; }
        public string? Context { get; set; }  // optional JSON payload or identifier
    }
}
