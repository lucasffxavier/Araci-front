using Araci.Models;

namespace Araci.Services
{
    public class ElectricGraphEdge
    {
        public ElectricGraphEdge(
            string edgeId,
            Cabo sourceCable,
            string fromElementId,
            string fromTerminalId,
            string toElementId,
            string toTerminalId,
            bool isValid,
            string? error)
        {
            EdgeId = edgeId;
            SourceCable = sourceCable;
            FromElementId = fromElementId;
            FromTerminalId = fromTerminalId;
            ToElementId = toElementId;
            ToTerminalId = toTerminalId;
            IsValid = isValid;
            Error = error;
        }

        public string EdgeId { get; }
        public Cabo SourceCable { get; }
        public string FromElementId { get; }
        public string FromTerminalId { get; }
        public string ToElementId { get; }
        public string ToTerminalId { get; }
        public bool IsValid { get; }
        public string? Error { get; }
    }
}
