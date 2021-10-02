using System;
using Serilog.Events;

namespace Serilog.Sinks.Models
{
    public class LiteDbLog 
    {
        public int Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public DateTime LogDate { get; set; }
        public LogEventLevel Level { get; set; }
        public string Exception { get; set; }
        public string RenderedMessage { get; set; }
        public string Properties { get; set; }
    }
}