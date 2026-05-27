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
            return elemento is ITerminalOwner and not Cabo;
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

            foreach (Cabo cabo in _document.Elementos.OfType<Cabo>())
                edges.Add(CreateEdge(cabo, nodeById, usedPairs));

            return edges;
        }

        private ElectricGraphEdge CreateEdge(
            Cabo cabo,
            IReadOnlyDictionary<string, ElectricGraphNode> nodeById,
            ISet<string> usedPairs)
        {
            string fromElementId = Normalize(cabo.OrigemId);
            string fromTerminalId = Normalize(cabo.OrigemTerminalId);
            string toElementId = Normalize(cabo.DestinoId);
            string toTerminalId = Normalize(cabo.DestinoTerminalId);
            var errors = new List<string>();

            ValidateEndpoint(
                "Origem",
                fromElementId,
                fromTerminalId,
                nodeById,
                errors);

            ValidateEndpoint(
                "Destino",
                toElementId,
                toTerminalId,
                nodeById,
                errors);

            if (!string.IsNullOrWhiteSpace(fromElementId) &&
                !string.IsNullOrWhiteSpace(toElementId) &&
                string.Equals(fromElementId, toElementId, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Origem e destino pertencem ao mesmo elemento.");
            }

            if (!string.IsNullOrWhiteSpace(fromElementId) &&
                !string.IsNullOrWhiteSpace(toElementId) &&
                !string.IsNullOrWhiteSpace(fromTerminalId) &&
                !string.IsNullOrWhiteSpace(toTerminalId) &&
                SameTerminal(fromElementId, fromTerminalId, toElementId, toTerminalId))
            {
                errors.Add("Origem e destino usam o mesmo terminal.");
            }

            if (EndpointComplete(fromElementId, fromTerminalId, toElementId, toTerminalId))
            {
                string key = PairKey(fromElementId, fromTerminalId, toElementId, toTerminalId);

                if (!usedPairs.Add(key))
                    errors.Add("Cabo duplicado entre os mesmos terminais.");
            }

            return new ElectricGraphEdge(
                cabo.Id.ToString(),
                cabo,
                fromElementId,
                fromTerminalId,
                toElementId,
                toTerminalId,
                errors.Count == 0,
                errors.Count == 0 ? null : string.Join(" ", errors));
        }

        private static void ValidateEndpoint(
            string label,
            string elementId,
            string terminalId,
            IReadOnlyDictionary<string, ElectricGraphNode> nodeById,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(elementId))
            {
                errors.Add($"Cabo sem {label}Id.");
                return;
            }

            if (!nodeById.TryGetValue(elementId, out ElectricGraphNode? node))
            {
                errors.Add($"Cabo com {label}Id inexistente: {elementId}.");
                return;
            }

            if (string.IsNullOrWhiteSpace(terminalId))
            {
                errors.Add($"Cabo sem {label}TerminalId.");
                return;
            }

            bool terminalExiste = node.Terminals.Any(t =>
                string.Equals(t.TerminalId, terminalId, StringComparison.OrdinalIgnoreCase));

            if (!terminalExiste)
                errors.Add($"Cabo com {label}TerminalId inexistente: {terminalId}.");
        }

        private static bool EndpointComplete(
            string fromElementId,
            string fromTerminalId,
            string toElementId,
            string toTerminalId)
        {
            return !string.IsNullOrWhiteSpace(fromElementId) &&
                !string.IsNullOrWhiteSpace(fromTerminalId) &&
                !string.IsNullOrWhiteSpace(toElementId) &&
                !string.IsNullOrWhiteSpace(toTerminalId);
        }

        private static string PairKey(
            string fromElementId,
            string fromTerminalId,
            string toElementId,
            string toTerminalId)
        {
            string a = $"{fromElementId.Trim()}:{fromTerminalId.Trim()}";
            string b = $"{toElementId.Trim()}:{toTerminalId.Trim()}";

            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) <= 0
                ? $"{a}|{b}"
                : $"{b}|{a}";
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

        private static string Normalize(string value)
        {
            return value?.Trim() ?? string.Empty;
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
