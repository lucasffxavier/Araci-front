using Araci.Models;

namespace Araci.Services
{
    public class TopologyIssue
    {
        public TopologyIssue(
            TopologyIssueSeverity severity,
            string message,
            Elemento? elemento = null)
        {
            Severity = severity;
            Message = message;
            Elemento = elemento;
        }

        public TopologyIssueSeverity Severity { get; }
        public string Message { get; }
        public Elemento? Elemento { get; }
    }
}
