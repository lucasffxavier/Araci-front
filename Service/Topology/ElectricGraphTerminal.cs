using Araci.Models;

namespace Araci.Services.Topology
{
    public class ElectricGraphTerminal
    {
        public ElectricGraphTerminal(
            string elementId,
            string terminalId,
            string? busName,
            Terminal sourceTerminal)
        {
            ElementId = elementId;
            TerminalId = terminalId;
            BusName = busName;
            SourceTerminal = sourceTerminal;
            Endpoint = new TerminalEndpoint(elementId, terminalId);
        }

        public string ElementId { get; }
        public string TerminalId { get; }
        public string? BusName { get; }
        public TerminalEndpoint Endpoint { get; }
        public Terminal SourceTerminal { get; }
    }
}
