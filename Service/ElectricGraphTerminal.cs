using Araci.Models;

namespace Araci.Services
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
        }

        public string ElementId { get; }
        public string TerminalId { get; }
        public string? BusName { get; }
        public Terminal SourceTerminal { get; }
    }
}
