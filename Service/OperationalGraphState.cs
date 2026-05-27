using System;
using System.Collections.Generic;
using System.Linq;

namespace Araci.Services
{
    public class OperationalGraphState
    {
        public OperationalGraphState(
            IEnumerable<string> energizedNodeIds,
            IEnumerable<string> deenergizedNodeIds,
            IEnumerable<string> energizedEdgeIds,
            IEnumerable<string> deenergizedEdgeIds,
            IEnumerable<string> sourceNodeIds)
        {
            EnergizedNodeIds = ToSet(energizedNodeIds);
            DeenergizedNodeIds = ToSet(deenergizedNodeIds);
            EnergizedEdgeIds = ToSet(energizedEdgeIds);
            DeenergizedEdgeIds = ToSet(deenergizedEdgeIds);
            SourceNodeIds = sourceNodeIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToList();
        }

        public IReadOnlySet<string> EnergizedNodeIds { get; }

        public IReadOnlySet<string> DeenergizedNodeIds { get; }

        public IReadOnlySet<string> EnergizedEdgeIds { get; }

        public IReadOnlySet<string> DeenergizedEdgeIds { get; }

        public IReadOnlyList<string> SourceNodeIds { get; }

        public bool IsNodeEnergized(string elementId)
        {
            return !string.IsNullOrWhiteSpace(elementId) &&
                EnergizedNodeIds.Contains(elementId.Trim());
        }

        public bool IsEdgeEnergized(string edgeId)
        {
            return !string.IsNullOrWhiteSpace(edgeId) &&
                EnergizedEdgeIds.Contains(edgeId.Trim());
        }

        private static IReadOnlySet<string> ToSet(IEnumerable<string> values)
        {
            return values
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
