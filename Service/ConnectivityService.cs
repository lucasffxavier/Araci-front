using System;
using System.Linq;
using Araci.Core.Documents;
using Araci.Models;

namespace Araci.Services
{
    public class ConnectivityService
    {
        private readonly AraciDocument _document;

        public ConnectivityService(EditorContext context)
            : this(context?.Document ?? throw new ArgumentNullException(nameof(context)))
        {
        }

        public ConnectivityService(AraciDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public Elemento? ObterElementoPorId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _document.Elementos.FirstOrDefault(e =>
                string.Equals(e.Id.ToString(), id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public string ResolverBusName(Elemento elemento)
        {
            if (!string.IsNullOrWhiteSpace(elemento.Nome))
                return elemento.Nome.Trim();

            string id = elemento.Id.ToString("N");
            return id.Length >= 8 ? $"BUS-{id[..8]}" : $"BUS-{id}";
        }

        public string ResolverBusNamePorId(string id)
        {
            Elemento? elemento = ObterElementoPorId(id);
            return elemento == null ? string.Empty : ResolverBusName(elemento);
        }

        public string ResolverBusNameParaEquipamento(ElementoEquipamento equipamento)
        {
            if (!string.IsNullOrWhiteSpace(equipamento.BarraId))
            {
                string busPorId = ResolverBusNamePorId(equipamento.BarraId);

                if (!string.IsNullOrWhiteSpace(busPorId))
                    return busPorId;
            }

            if (!string.IsNullOrWhiteSpace(equipamento.Barra))
                return equipamento.Barra.Trim();

            return ResolverBusName(equipamento);
        }

        public string ResolverBus1(Cabo cabo)
        {
            if (!string.IsNullOrWhiteSpace(cabo.OrigemId))
            {
                string busPorId = ResolverBusNamePorId(cabo.OrigemId);

                if (!string.IsNullOrWhiteSpace(busPorId))
                    return busPorId;
            }

            return cabo.BarraOrigem?.Trim() ?? string.Empty;
        }

        public string ResolverBus2(Cabo cabo)
        {
            if (!string.IsNullOrWhiteSpace(cabo.DestinoId))
            {
                string busPorId = ResolverBusNamePorId(cabo.DestinoId);

                if (!string.IsNullOrWhiteSpace(busPorId))
                    return busPorId;
            }

            return cabo.BarraDestino?.Trim() ?? string.Empty;
        }
    }
}
