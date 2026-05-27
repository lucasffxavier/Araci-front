using System;
using System.Linq;
using Araci.Core.Documents;
using Araci.Models;

namespace Araci.Services
{
    public class TopologyValidator
    {
        private readonly AraciDocument _document;
        private readonly ConnectivityService _connectivity;
        private readonly ElectricGraphBuilder _graphBuilder;

        public TopologyValidator(EditorContext context)
            : this(
                context?.Document ?? throw new ArgumentNullException(nameof(context)),
                context.ElectricGraph)
        {
        }

        public TopologyValidator(AraciDocument document)
            : this(document, new ElectricGraphBuilder(document))
        {
        }

        private TopologyValidator(
            AraciDocument document,
            ElectricGraphBuilder graphBuilder)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _connectivity = new ConnectivityService(_document);
            _graphBuilder = graphBuilder ?? throw new ArgumentNullException(nameof(graphBuilder));
        }

        public TopologyValidationResult Validate()
        {
            var result = new TopologyValidationResult();

            ValidarNomes(result);
            ValidarCabos(result);
            ValidarEquipamentos(result);
            ValidarCircuito(result);

            return result;
        }

        private void ValidarNomes(TopologyValidationResult result)
        {
            foreach (Elemento elemento in _document.Elementos)
            {
                if (string.IsNullOrWhiteSpace(elemento.Nome))
                    result.AddError($"Elemento {elemento.Id} sem Nome.");
            }

            var duplicados = _document.Elementos
                .Where(e => !string.IsNullOrWhiteSpace(e.Nome))
                .GroupBy(e => e.Nome.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var grupo in duplicados)
                result.AddError($"Nome duplicado no documento: '{grupo.Key}'.");
        }

        private void ValidarCabos(TopologyValidationResult result)
        {
            ElectricGraph graph = _graphBuilder.Build();

            foreach (ElectricGraphEdge edge in graph.GetInvalidEdges())
            {
                result.AddError(
                    $"Cabo '{Nome(edge.SourceCable)}': {edge.Error}");
            }
        }

        private void ValidarEquipamentos(TopologyValidationResult result)
        {
            foreach (ElementoEquipamento equipamento in _document.Elementos.OfType<ElementoEquipamento>())
            {
                if (!string.IsNullOrWhiteSpace(equipamento.BarraId) &&
                    _connectivity.ObterElementoPorId(equipamento.BarraId) == null)
                {
                    result.AddError($"Equipamento '{Nome(equipamento)}' com BarraId invalido: {equipamento.BarraId}.");
                }
            }

            foreach (Carga carga in _document.Elementos.OfType<Carga>())
            {
                if (!TemConexaoTopologica(carga))
                    result.AddError($"Carga '{Nome(carga)}' sem conexao topologica utilizavel por Id.");
            }

            foreach (Gerador gerador in _document.Elementos.OfType<Gerador>())
            {
                if (!TemConexaoTopologica(gerador))
                    result.AddError($"Gerador '{Nome(gerador)}' sem conexao topologica utilizavel por Id.");
            }
        }

        private void ValidarCircuito(TopologyValidationResult result)
        {
            int equipamentos = _document.Elementos.OfType<ElementoEquipamento>().Count();

            if (!_document.Elementos.OfType<Sin>().Any() &&
                !_document.Elementos.OfType<Gerador>().Any())
            {
                result.AddError("Circuito sem fonte slack.");
            }

            if (!_document.Elementos.OfType<Carga>().Any())
                result.AddError("Circuito sem carga.");

            if (equipamentos > 1 && !_document.Elementos.OfType<Cabo>().Any())
                result.AddError("Circuito com mais de um equipamento e sem cabo.");
        }

        private bool TemConexaoTopologica(ElementoEquipamento equipamento)
        {
            if (!string.IsNullOrWhiteSpace(equipamento.BarraId) &&
                _connectivity.ObterElementoPorId(equipamento.BarraId) != null)
            {
                return true;
            }

            string id = equipamento.Id.ToString();

            return _document.Elementos.OfType<Cabo>().Any(c =>
                string.Equals(c.OrigemId, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.DestinoId, id, StringComparison.OrdinalIgnoreCase));
        }

        private static string Nome(Elemento elemento)
        {
            return string.IsNullOrWhiteSpace(elemento.Nome)
                ? elemento.Id.ToString()
                : elemento.Nome.Trim();
        }
    }
}
