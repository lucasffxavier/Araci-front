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

        public ElectricGraphNode? FindNodeByElementId(string elementId)
        {
            return FindNode(elementId);
        }

        public ElectricGraphNode? FindNodeByElement(Elemento elemento)
        {
            return elemento == null ? null : FindNodeByElementId(elemento.Id.ToString());
        }

        public ElectricGraphTerminal? FindTerminal(string elementId, string terminalId)
        {
            return FindTerminal(new TerminalEndpoint(elementId, terminalId));
        }

        public ElectricGraphTerminal? FindTerminal(TerminalEndpoint endpoint)
        {
            if (!endpoint.IsComplete)
                return null;

            return FindNode(endpoint.ElementId)?.Terminals.FirstOrDefault(t =>
                t.Endpoint == endpoint);
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
            return GetEdgesForTerminal(new TerminalEndpoint(elementId, terminalId));
        }

        public IReadOnlyList<ElectricGraphEdge> GetEdgesForTerminal(TerminalEndpoint endpoint)
        {
            if (!endpoint.IsComplete)
                return Array.Empty<ElectricGraphEdge>();

            return Edges.Where(e =>
                e.From == endpoint ||
                e.To == endpoint)
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

        public IReadOnlyList<ElectricGraphNode> BreadthFirst(string startElementId)
        {
            ElectricGraphNode? start = FindNode(startElementId);

            if (start == null)
                return Array.Empty<ElectricGraphNode>();

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<ElectricGraphNode>();
            var result = new List<ElectricGraphNode>();

            visited.Add(start.ElementId);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                ElectricGraphNode current = queue.Dequeue();
                result.Add(current);

                foreach (ElectricGraphNode neighbor in GetNeighbors(current.ElementId))
                {
                    if (visited.Add(neighbor.ElementId))
                        queue.Enqueue(neighbor);
                }
            }

            return result;
        }
    }
}
