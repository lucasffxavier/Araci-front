using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Models;

namespace Araci.Services
{
    public class OperationalGraphStateBuilder
    {
        public OperationalGraphState Build(ElectricGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            List<string> sourceNodeIds = GetSourceNodeIds(graph);
            var energizedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var energizedEdgeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();

            foreach (string sourceId in sourceNodeIds)
            {
                if (energizedNodeIds.Add(sourceId))
                    queue.Enqueue(sourceId);
            }

            while (queue.Count > 0)
            {
                string currentId = queue.Dequeue();

                foreach (ElectricGraphEdge edge in graph.GetEdgesForElement(currentId).Where(e => e.IsValid))
                {
                    energizedEdgeIds.Add(edge.EdgeId);

                    string otherId = OtherEndpoint(edge, currentId);

                    if (graph.FindNode(otherId) != null && energizedNodeIds.Add(otherId))
                        queue.Enqueue(otherId);
                }
            }

            var allNodeIds = graph.Nodes.Select(n => n.ElementId).ToList();
            var allEdgeIds = graph.Edges.Select(e => e.EdgeId).ToList();

            return new OperationalGraphState(
                energizedNodeIds,
                allNodeIds.Where(id => !energizedNodeIds.Contains(id)),
                energizedEdgeIds,
                allEdgeIds.Where(id => !energizedEdgeIds.Contains(id)),
                sourceNodeIds);
        }

        private static List<string> GetSourceNodeIds(ElectricGraph graph)
        {
            var sinSources = graph.Nodes
                .Where(n => n.SourceElement is Sin)
                .Select(n => n.ElementId)
                .ToList();

            if (sinSources.Count > 0)
                return sinSources;

            ElectricGraphNode? generator = graph.Nodes.FirstOrDefault(n => n.SourceElement is Gerador);
            return generator == null ? new List<string>() : new List<string> { generator.ElementId };
        }

        private static string OtherEndpoint(ElectricGraphEdge edge, string elementId)
        {
            return string.Equals(edge.FromElementId, elementId, StringComparison.OrdinalIgnoreCase)
                ? edge.ToElementId
                : edge.FromElementId;
        }
    }
}
