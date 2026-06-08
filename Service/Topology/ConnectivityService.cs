using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services.Interaction;

namespace Araci.Services.Topology
{
    public class ConnectivityService
    {
        private readonly AraciDocument _document;

        public ConnectivityService(AraciDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public Elemento? ObterElementoPorId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return ObterElementoPorIdNaVistaAtiva(id);
        }

        public Terminal? ObterTerminal(TerminalEndpoint endpoint)
        {
            if (!endpoint.IsComplete)
                return null;

            Elemento? elemento = ObterElementoPorId(endpoint.ElementId);
            return elemento == null ? null : ObterTerminal(elemento, endpoint.TerminalId);
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
            Elemento? elemento = ObterElementoPorIdNaVistaAtiva(id);
            return elemento == null ? string.Empty : ResolverBusName(elemento);
        }

        public string ResolverBusNamePorIdEstrito(string id, string contexto)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException($"{contexto} sem Id de conexao.");

            Elemento? elemento = ObterElementoPorIdNaVistaAtiva(id);

            if (elemento == null)
                throw new InvalidOperationException($"{contexto} aponta para Id inexistente: {id}.");

            return ResolverBusName(elemento);
        }

        public string ResolverBusNameParaEquipamento(ElementoEquipamento equipamento)
        {
            if (!string.IsNullOrWhiteSpace(equipamento.BarraId))
            {
                string busPorId = ResolverBusNamePorIdNaVista(equipamento.BarraId, equipamento);

                if (!string.IsNullOrWhiteSpace(busPorId))
                    return busPorId;
            }

            if (!string.IsNullOrWhiteSpace(equipamento.Barra))
                return equipamento.Barra.Trim();

            return ResolverBusName(equipamento);
        }

        public string ResolverBusNameParaEquipamentoEstrito(ElementoEquipamento equipamento)
        {
            if (!string.IsNullOrWhiteSpace(equipamento.BarraId))
                return ResolverBusNamePorIdEstritoNaVista(equipamento.BarraId, $"Equipamento '{ResolverBusName(equipamento)}'", equipamento);

            return ResolverBusName(equipamento);
        }

        public string ResolverBus1(Cabo cabo)
        {
            if (!string.IsNullOrWhiteSpace(cabo.OrigemId))
            {
                string busPorId = ResolverBusNamePorIdNaVista(cabo.OrigemId, cabo);

                if (!string.IsNullOrWhiteSpace(busPorId))
                    return busPorId;
            }

            return cabo.BarraOrigem?.Trim() ?? string.Empty;
        }

        public string ResolverBus1Estrito(Cabo cabo)
        {
            return ResolverBusNamePorIdEstritoNaVista(cabo.OrigemId, $"Cabo '{ResolverBusName(cabo)}' origem", cabo);
        }

        public string ResolverBus2(Cabo cabo)
        {
            if (!string.IsNullOrWhiteSpace(cabo.DestinoId))
            {
                string busPorId = ResolverBusNamePorIdNaVista(cabo.DestinoId, cabo);

                if (!string.IsNullOrWhiteSpace(busPorId))
                    return busPorId;
            }

            return cabo.BarraDestino?.Trim() ?? string.Empty;
        }

        public string ResolverBus2Estrito(Cabo cabo)
        {
            return ResolverBusNamePorIdEstritoNaVista(cabo.DestinoId, $"Cabo '{ResolverBusName(cabo)}' destino", cabo);
        }

        public IReadOnlyList<Cabo> ObterCabosConectados(Elemento elemento)
        {
            string id = elemento.Id.ToString();

            return _document.ObterElementosDaVistaDoElementoOuAtiva(elemento)
                .OfType<Cabo>()
                .Where(c =>
                    string.Equals(c.OrigemId, id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.DestinoId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IReadOnlySet<string> ObterTerminalIdsOcupados(Elemento elemento)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string id = elemento.Id.ToString();

            foreach (Cabo cabo in ObterCabosConectados(elemento))
            {
                if (string.Equals(cabo.OrigemId, id, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(cabo.OrigemTerminalId))
                {
                    ids.Add(cabo.OrigemTerminalId);
                }

                if (string.Equals(cabo.DestinoId, id, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(cabo.DestinoTerminalId))
                {
                    ids.Add(cabo.DestinoTerminalId);
                }
            }

            return ids;
        }

        public IReadOnlyList<Cabo> ObterCabosConectados(TerminalEndpoint endpoint)
        {
            if (!endpoint.IsComplete)
                return Array.Empty<Cabo>();

            return _document.ObterElementosDaVistaAtiva()
                .OfType<Cabo>()
                .Where(c => c.OrigemEndpoint == endpoint || c.DestinoEndpoint == endpoint)
                .ToList();
        }

        public IReadOnlyList<Cabo> ReancorarCabosConectados(Elemento elemento)
        {
            var alterados = new List<Cabo>();

            foreach (Cabo cabo in ObterCabosConectados(elemento))
            {
                bool alterou = false;

                if (EhMesmoElemento(cabo.OrigemId, elemento) && ReancorarOrigem(cabo, elemento))
                    alterou = true;

                if (EhMesmoElemento(cabo.DestinoId, elemento) && ReancorarDestino(cabo, elemento))
                    alterou = true;

                if (alterou)
                    alterados.Add(cabo);
            }

            return alterados;
        }

        public ConnectionValidationResult ValidarTerminalDisponivel(Cabo? caboAtual, Terminal? terminal)
        {
            if (terminal == null)
                return ConnectionValidationResult.Invalid("Conexão inválida");

            TerminalEndpoint endpoint = TerminalEndpoint.FromTerminal(terminal);

            if (!endpoint.IsComplete)
                return ConnectionValidationResult.Invalid("Conexão inválida");

            return ExisteCaboNoTerminal(caboAtual, endpoint)
                ? ConnectionValidationResult.Invalid("Conexão inválida")
                : ConnectionValidationResult.Valid();
        }

        public bool TerminalEstaOcupado(Terminal terminal, Cabo? caboIgnorado = null)
        {
            TerminalEndpoint endpoint = TerminalEndpoint.FromTerminal(terminal);
            return endpoint.IsComplete && ExisteCaboNoTerminal(caboIgnorado, endpoint);
        }

        public ConnectionValidationResult ValidarConexaoCabo(Cabo? caboAtual, Terminal? origem, Terminal? destino)
        {
            if (origem == null || destino == null)
                return ConnectionValidationResult.Invalid("Conexão inválida");

            TerminalEndpoint origemEndpoint = TerminalEndpoint.FromTerminal(origem);
            TerminalEndpoint destinoEndpoint = TerminalEndpoint.FromTerminal(destino);

            if (!origemEndpoint.IsComplete || !destinoEndpoint.IsComplete)
                return ConnectionValidationResult.Invalid("Conexão inválida");

            if (string.Equals(origemEndpoint.ElementId, destinoEndpoint.ElementId, StringComparison.OrdinalIgnoreCase))
                return ConnectionValidationResult.Invalid("Conexão inválida");

            if (origemEndpoint == destinoEndpoint)
                return ConnectionValidationResult.Invalid("Conexão inválida");

            if (ExisteCaboNoTerminal(caboAtual, origemEndpoint) || ExisteCaboNoTerminal(caboAtual, destinoEndpoint))
                return ConnectionValidationResult.Invalid("Conexão ocupada");

            if (ExisteCaboDuplicado(caboAtual, origemEndpoint, destinoEndpoint))
                return ConnectionValidationResult.Invalid("Conexão inválida");

            return ConnectionValidationResult.Valid();
        }

        public bool EhCaboDuplicado(Cabo cabo, IEnumerable<Cabo> anteriores)
        {
            foreach (Cabo anterior in anteriores)
            {
                if (MesmoParDeTerminais(cabo.OrigemEndpoint, cabo.DestinoEndpoint, anterior.OrigemEndpoint, anterior.DestinoEndpoint))
                    return true;
            }

            return false;
        }

        private static bool EhMesmoElemento(string id, Elemento elemento)
        {
            return string.Equals(id, elemento.Id.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool ExisteCaboNoTerminal(Cabo? caboAtual, TerminalEndpoint endpoint)
        {
            if (!endpoint.IsComplete)
                return false;

            return ObterCabosDoEscopo(caboAtual).Any(c =>
                !ReferenceEquals(c, caboAtual) &&
                (c.OrigemEndpoint == endpoint || c.DestinoEndpoint == endpoint));
        }

        private bool ExisteCaboDuplicado(Cabo? caboAtual, TerminalEndpoint origem, TerminalEndpoint destino)
        {
            return ObterCabosDoEscopo(caboAtual).Any(c =>
                !ReferenceEquals(c, caboAtual) &&
                MesmoParDeTerminais(origem, destino, c.OrigemEndpoint, c.DestinoEndpoint));
        }

        private Elemento? ObterElementoPorIdNaVistaAtiva(string id)
        {
            return ObterElementoPorId(_document.ObterElementosDaVistaAtiva(), id);
        }

        private Elemento? ObterElementoPorIdNaVistaDoElemento(Elemento referencia, string id)
        {
            return ObterElementoPorId(_document.ObterElementosDaVistaDoElementoOuAtiva(referencia), id);
        }

        private static Elemento? ObterElementoPorId(IEnumerable<Elemento> elementos, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return elementos.FirstOrDefault(e =>
                string.Equals(e.Id.ToString(), id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private string ResolverBusNamePorIdNaVista(string id, Elemento referencia)
        {
            Elemento? elemento = ObterElementoPorIdNaVistaDoElemento(referencia, id);
            return elemento == null ? string.Empty : ResolverBusName(elemento);
        }

        private string ResolverBusNamePorIdEstritoNaVista(string id, string contexto, Elemento referencia)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException($"{contexto} sem Id de conexao.");

            Elemento? elemento = ObterElementoPorIdNaVistaDoElemento(referencia, id);

            if (elemento == null)
                throw new InvalidOperationException($"{contexto} aponta para Id inexistente: {id}.");

            return ResolverBusName(elemento);
        }

        private IEnumerable<Cabo> ObterCabosDoEscopo(Cabo? caboAtual)
        {
            return _document.ObterElementosDaVistaDoElementoOuAtiva(caboAtual).OfType<Cabo>();
        }

        private static bool MesmoParDeTerminais(TerminalEndpoint origemA, TerminalEndpoint destinoA, TerminalEndpoint origemB, TerminalEndpoint destinoB)
        {
            if (!origemA.IsComplete || !destinoA.IsComplete || !origemB.IsComplete || !destinoB.IsComplete)
                return false;

            return string.Equals(TerminalEndpoint.PairKey(origemA, destinoA), TerminalEndpoint.PairKey(origemB, destinoB), StringComparison.OrdinalIgnoreCase);
        }

        private static bool ReancorarOrigem(Cabo cabo, Elemento elemento)
        {
            Terminal? terminal = ObterTerminal(elemento, cabo.OrigemTerminalId);

            if (terminal == null)
                return false;

            if (cabo.Vertices.Count == 0)
                cabo.Vertices.Add(terminal.Posicao);
            else
                cabo.Vertices[0] = terminal.Posicao;

            cabo.DefinirOrigem(terminal.Posicao);
            return true;
        }

        private static bool ReancorarDestino(Cabo cabo, Elemento elemento)
        {
            Terminal? terminal = ObterTerminal(elemento, cabo.DestinoTerminalId);

            if (terminal == null)
                return false;

            if (cabo.Vertices.Count == 0)
                cabo.Vertices.Add(terminal.Posicao);
            else if (cabo.Vertices.Count == 1)
                cabo.Vertices.Add(terminal.Posicao);
            else
                cabo.Vertices[^1] = terminal.Posicao;

            cabo.DefinirDestino(terminal.Posicao);
            return true;
        }

        private static Terminal? ObterTerminal(Elemento elemento, string terminalId)
        {
            if (elemento is not ITerminalOwner owner || string.IsNullOrWhiteSpace(terminalId))
                return null;

            return owner.Terminais.FirstOrDefault(t =>
                string.Equals(t.Id, terminalId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
