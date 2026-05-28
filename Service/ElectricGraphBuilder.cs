using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Documents;
using Araci.Models;

namespace Araci.Services
{
    public class ElectricGraphBuilder
    {
        private readonly AraciDocument _document;
        private readonly ElementRegistryService? _registry;

        public ElectricGraphBuilder(AraciDocument document)
            : this(document, null)
        {
        }

        public ElectricGraphBuilder(EditorContext context)
            : this(
                context?.Document ?? throw new ArgumentNullException(nameof(context)),
                context.Elements)
        {
        }

        private ElectricGraphBuilder(
            AraciDocument document,
            ElementRegistryService? registry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _registry = registry;
        }

        public ElectricGraph Build()
        {
            var nodes = _document.Elementos
                .Where(IsNodeElement)
                .Select(CreateNode)
                .ToList();

            var nodeById = nodes.ToDictionary(
                n => n.ElementId,
                StringComparer.OrdinalIgnoreCase);

            var edges = BuildEdges(nodeById);

            return new ElectricGraph(nodes, edges);
        }

        private static bool IsNodeElement(Elemento elemento)
        {
            return elemento.ParticipaDoGrafoEletrico &&
                elemento is ITerminalOwner and not Cabo;
        }

        private ElectricGraphNode CreateNode(Elemento elemento)
        {
            string elementId = elemento.Id.ToString();
            var terminals = ((ITerminalOwner)elemento).Terminais
                .Select(t => new ElectricGraphTerminal(
                    elementId,
                    t.Id,
                    string.IsNullOrWhiteSpace(t.Barra) ? ResolveBusName(elemento) : t.Barra,
                    t))
                .ToList();

            return new ElectricGraphNode(
                elementId,
                elemento.Id,
                ResolveBusName(elemento),
                _registry?.GetKind(elemento) ?? elemento.GetType().Name,
                elemento,
                terminals);
        }

        private IReadOnlyList<ElectricGraphEdge> BuildEdges(
            IReadOnlyDictionary<string, ElectricGraphNode> nodeById)
        {
            var edges = new List<ElectricGraphEdge>();
            var usedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Cabo cabo in _document.Elementos.OfType<Cabo>().Where(c => c.ParticipaDoGrafoEletrico))
                edges.Add(CreateEdge(cabo, nodeById, usedPairs));

            return edges;
        }

        private ElectricGraphEdge CreateEdge(
            Cabo cabo,
            IReadOnlyDictionary<string, ElectricGraphNode> nodeById,
            ISet<string> usedPairs)
        {
            TerminalEndpoint from = cabo.OrigemEndpoint;
            TerminalEndpoint to = cabo.DestinoEndpoint;
            var errors = new List<string>();

            ValidateEndpoint(
                "Origem",
                from,
                nodeById,
                errors);

            ValidateEndpoint(
                "Destino",
                to,
                nodeById,
                errors);

            if (!string.IsNullOrWhiteSpace(from.ElementId) &&
                !string.IsNullOrWhiteSpace(to.ElementId) &&
                string.Equals(from.ElementId, to.ElementId, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Origem e destino pertencem ao mesmo elemento.");
            }

            if (from.IsComplete &&
                to.IsComplete &&
                from == to)
            {
                errors.Add("Origem e destino usam o mesmo terminal.");
            }

            if (from.IsComplete && to.IsComplete)
            {
                string key = TerminalEndpoint.PairKey(from, to);

                if (!usedPairs.Add(key))
                    errors.Add("Cabo duplicado entre os mesmos terminais.");
            }

            return new ElectricGraphEdge(
                cabo.Id.ToString(),
                cabo,
                from.ElementId,
                from.TerminalId,
                to.ElementId,
                to.TerminalId,
                errors.Count == 0,
                errors.Count == 0 ? null : string.Join(" ", errors));
        }

        private static void ValidateEndpoint(
            string label,
            TerminalEndpoint endpoint,
            IReadOnlyDictionary<string, ElectricGraphNode> nodeById,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(endpoint.ElementId))
            {
                errors.Add($"Cabo sem {label}Id.");
                return;
            }

            if (!nodeById.TryGetValue(endpoint.ElementId, out ElectricGraphNode? node))
            {
                errors.Add($"Cabo com {label}Id inexistente: {endpoint.ElementId}.");
                return;
            }

            if (string.IsNullOrWhiteSpace(endpoint.TerminalId))
            {
                errors.Add($"Cabo sem {label}TerminalId.");
                return;
            }

            bool terminalExiste = node.Terminals.Any(t =>
                t.Endpoint == endpoint);

            if (!terminalExiste)
                errors.Add($"Cabo com {label}TerminalId inexistente: {endpoint.TerminalId}.");
        }

        private static string ResolveBusName(Elemento elemento)
        {
            if (!string.IsNullOrWhiteSpace(elemento.Nome))
                return elemento.Nome.Trim();

            string id = elemento.Id.ToString("N");
            return id.Length >= 8 ? $"BUS-{id[..8]}" : $"BUS-{id}";
        }
    }
}
