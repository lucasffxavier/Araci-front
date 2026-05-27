using System;

namespace Araci.Models
{
    public readonly struct TerminalEndpoint : IEquatable<TerminalEndpoint>
    {
        public TerminalEndpoint(string elementId, string terminalId)
        {
            ElementId = Normalize(elementId);
            TerminalId = Normalize(terminalId);
        }

        public string ElementId { get; }

        public string TerminalId { get; }

        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(ElementId) &&
            !string.IsNullOrWhiteSpace(TerminalId);

        public static TerminalEndpoint Empty { get; } = new(string.Empty, string.Empty);

        public static TerminalEndpoint FromTerminal(Terminal? terminal)
        {
            return terminal == null
                ? Empty
                : new TerminalEndpoint(terminal.Dono.Id.ToString(), terminal.Id);
        }

        public bool Equals(TerminalEndpoint other)
        {
            return string.Equals(ElementId, other.ElementId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TerminalId, other.TerminalId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is TerminalEndpoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(ElementId),
                StringComparer.OrdinalIgnoreCase.GetHashCode(TerminalId));
        }

        public override string ToString()
        {
            return IsComplete ? $"{ElementId}:{TerminalId}" : string.Empty;
        }

        public static bool operator ==(TerminalEndpoint left, TerminalEndpoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TerminalEndpoint left, TerminalEndpoint right)
        {
            return !left.Equals(right);
        }

        public static string PairKey(TerminalEndpoint left, TerminalEndpoint right)
        {
            string a = left.ToString();
            string b = right.ToString();

            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) <= 0
                ? $"{a}|{b}"
                : $"{b}|{a}";
        }

        private static string Normalize(string value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}
