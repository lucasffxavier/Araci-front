using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Models;

namespace Araci.Services
{
    public class ElectricGraph
    {
        private readonly Dictionary<string, ElectricGraphNode> _nodesById;

        public ElectricGraph(
            IReadOnlyList<ElectricGraphNode> nodes,
            IReadOnlyList<ElectricGraphEdge> edges)
        {
            Nodes = nodes;
            Edges = edges;
            _nodesById = nodes.ToDictionary(
                n => n.ElementId,
                StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<ElectricGraphNode> Nodes { get; }
        public IReadOnlyList<ElectricGraphEdge> Edges { get; }

        public ElectricGraphNode? FindNode(string elementId)
        {
            return string.IsNullOrWhiteSpace(elementId)
                ? null
                : _nodesById.TryGetValue(elementId.Trim(), out ElectricGraphNode? node)
                    ? node
                    : null;
        }

        public ElectricGraphTerminal? FindTerminal(string elementId, string terminalId)
        {
            if (string.IsNullOrWhiteSpace(terminalId))
                return null;

            return FindNode(elementId)?.Terminals.FirstOrDefault(t =>
                string.Equals(t.TerminalId, terminalId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<ElectricGraphEdge> GetEdgesForElement(string elementId)
        {
            if (string.IsNullOrWhiteSpace(elementId))
                return Array.Empty<ElectricGraphEdge>();

            string id = elementId.Trim();

            return Edges.Where(e =>
                string.Equals(e.FromElementId, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.ToElementId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IReadOnlyList<ElectricGraphEdge> GetEdgesForTerminal(
            string elementId,
            string terminalId)
        {
            if (string.IsNullOrWhiteSpace(elementId) ||
                string.IsNullOrWhiteSpace(terminalId))
            {
                return Array.Empty<ElectricGraphEdge>();
            }

            string element = elementId.Trim();
            string terminal = terminalId.Trim();

            return Edges.Where(e =>
                SameTerminal(e.FromElementId, e.FromTerminalId, element, terminal) ||
                SameTerminal(e.ToElementId, e.ToTerminalId, element, terminal))
                .ToList();
        }

        public IReadOnlyList<ElectricGraphNode> GetNeighbors(string elementId)
        {
            var neighbors = new List<ElectricGraphNode>();

            foreach (ElectricGraphEdge edge in GetEdgesForElement(elementId).Where(e => e.IsValid))
            {
                string otherId = string.Equals(edge.FromElementId, elementId, StringComparison.OrdinalIgnoreCase)
                    ? edge.ToElementId
                    : edge.FromElementId;

                ElectricGraphNode? node = FindNode(otherId);

                if (node != null && !neighbors.Contains(node))
                    neighbors.Add(node);
            }

            return neighbors;
        }

        public IReadOnlyList<ElectricGraphEdge> GetInvalidEdges()
        {
            return Edges.Where(e => !e.IsValid).ToList();
        }

        public IReadOnlyList<ElectricGraphEdge> GetValidEdges()
        {
            return Edges.Where(e => e.IsValid).ToList();
        }

        public ElectricGraphEdge? FindEdgeByCableId(string cableId)
        {
            if (string.IsNullOrWhiteSpace(cableId))
                return null;

            string id = cableId.Trim();

            return Edges.FirstOrDefault(e =>
                string.Equals(e.EdgeId, id, StringComparison.OrdinalIgnoreCase));
        }

        public ElectricGraphEdge? FindEdgeByCable(Cabo cabo)
        {
            return cabo == null ? null : FindEdgeByCableId(cabo.Id.ToString());
        }

        private static bool SameTerminal(
            string elementA,
            string terminalA,
            string elementB,
            string terminalB)
        {
            return string.Equals(elementA, elementB, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(terminalA, terminalB, StringComparison.OrdinalIgnoreCase);
        }
    }
}
