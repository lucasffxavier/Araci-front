using System;
using System.Collections.Generic;
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

        public string ResolverBusNamePorIdEstrito(string id, string contexto)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException($"{contexto} sem Id de conexao.");

            Elemento? elemento = ObterElementoPorId(id);

            if (elemento == null)
                throw new InvalidOperationException($"{contexto} aponta para Id inexistente: {id}.");

            return ResolverBusName(elemento);
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

        public string ResolverBusNameParaEquipamentoEstrito(ElementoEquipamento equipamento)
        {
            if (!string.IsNullOrWhiteSpace(equipamento.BarraId))
                return ResolverBusNamePorIdEstrito(
                    equipamento.BarraId,
                    $"Equipamento '{ResolverBusName(equipamento)}'");

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

        public string ResolverBus1Estrito(Cabo cabo)
        {
            return ResolverBusNamePorIdEstrito(
                cabo.OrigemId,
                $"Cabo '{ResolverBusName(cabo)}' origem");
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

        public string ResolverBus2Estrito(Cabo cabo)
        {
            return ResolverBusNamePorIdEstrito(
                cabo.DestinoId,
                $"Cabo '{ResolverBusName(cabo)}' destino");
        }

        public IReadOnlyList<Cabo> ObterCabosConectados(Elemento elemento)
        {
            string id = elemento.Id.ToString();

            return _document.Elementos
                .OfType<Cabo>()
                .Where(c =>
                    string.Equals(c.OrigemId, id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.DestinoId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IReadOnlyList<Cabo> ReancorarCabosConectados(Elemento elemento)
        {
            var alterados = new List<Cabo>();

            foreach (Cabo cabo in ObterCabosConectados(elemento))
            {
                bool alterou = false;

                if (EhMesmoElemento(cabo.OrigemId, elemento) &&
                    ReancorarOrigem(cabo, elemento))
                {
                    alterou = true;
                }

                if (EhMesmoElemento(cabo.DestinoId, elemento) &&
                    ReancorarDestino(cabo, elemento))
                {
                    alterou = true;
                }

                if (alterou)
                    alterados.Add(cabo);
            }

            return alterados;
        }

        public ConnectionValidationResult ValidarConexaoCabo(
            Cabo? caboAtual,
            Terminal? origem,
            Terminal? destino)
        {
            if (origem == null)
                return ConnectionValidationResult.Invalid("Origem sem terminal valido.");

            if (destino == null)
                return ConnectionValidationResult.Invalid("Destino sem terminal valido.");

            if (string.IsNullOrWhiteSpace(origem.Dono.Id.ToString()) ||
                string.IsNullOrWhiteSpace(origem.Id) ||
                string.IsNullOrWhiteSpace(destino.Dono.Id.ToString()) ||
                string.IsNullOrWhiteSpace(destino.Id))
            {
                return ConnectionValidationResult.Invalid("Conexao sem ElementoId ou TerminalId valido.");
            }

            if (MesmoElemento(origem, destino))
                return ConnectionValidationResult.Invalid("Origem e destino pertencem ao mesmo elemento.");

            if (MesmoTerminal(origem, destino))
                return ConnectionValidationResult.Invalid("Origem e destino usam o mesmo terminal.");

            if (ExisteCaboDuplicado(caboAtual, origem, destino))
                return ConnectionValidationResult.Invalid("Ja existe cabo entre estes terminais.");

            return ConnectionValidationResult.Valid();
        }

        public bool EhCaboDuplicado(Cabo cabo, IEnumerable<Cabo> anteriores)
        {
            foreach (Cabo anterior in anteriores)
            {
                if (MesmoParDeTerminais(
                    cabo.OrigemId,
                    cabo.OrigemTerminalId,
                    cabo.DestinoId,
                    cabo.DestinoTerminalId,
                    anterior.OrigemId,
                    anterior.OrigemTerminalId,
                    anterior.DestinoId,
                    anterior.DestinoTerminalId))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EhMesmoElemento(string id, Elemento elemento)
        {
            return string.Equals(
                id,
                elemento.Id.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        private bool ExisteCaboDuplicado(
            Cabo? caboAtual,
            Terminal origem,
            Terminal destino)
        {
            string origemId = origem.Dono.Id.ToString();
            string destinoId = destino.Dono.Id.ToString();

            return _document.Elementos.OfType<Cabo>().Any(c =>
                !ReferenceEquals(c, caboAtual) &&
                MesmoParDeTerminais(
                    origemId,
                    origem.Id,
                    destinoId,
                    destino.Id,
                    c.OrigemId,
                    c.OrigemTerminalId,
                    c.DestinoId,
                    c.DestinoTerminalId));
        }

        private static bool MesmoElemento(Terminal origem, Terminal destino)
        {
            return string.Equals(
                origem.Dono.Id.ToString(),
                destino.Dono.Id.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool MesmoTerminal(Terminal origem, Terminal destino)
        {
            return MesmoElemento(origem, destino) &&
                string.Equals(origem.Id, destino.Id, StringComparison.OrdinalIgnoreCase);
        }

        private static bool MesmoParDeTerminais(
            string origemA,
            string origemTerminalA,
            string destinoA,
            string destinoTerminalA,
            string origemB,
            string origemTerminalB,
            string destinoB,
            string destinoTerminalB)
        {
            if (string.IsNullOrWhiteSpace(origemA) ||
                string.IsNullOrWhiteSpace(origemTerminalA) ||
                string.IsNullOrWhiteSpace(destinoA) ||
                string.IsNullOrWhiteSpace(destinoTerminalA) ||
                string.IsNullOrWhiteSpace(origemB) ||
                string.IsNullOrWhiteSpace(origemTerminalB) ||
                string.IsNullOrWhiteSpace(destinoB) ||
                string.IsNullOrWhiteSpace(destinoTerminalB))
            {
                return false;
            }

            bool direto =
                MesmoPonto(origemA, origemTerminalA, origemB, origemTerminalB) &&
                MesmoPonto(destinoA, destinoTerminalA, destinoB, destinoTerminalB);

            bool invertido =
                MesmoPonto(origemA, origemTerminalA, destinoB, destinoTerminalB) &&
                MesmoPonto(destinoA, destinoTerminalA, origemB, origemTerminalB);

            return direto || invertido;
        }

        private static bool MesmoPonto(
            string elementoA,
            string terminalA,
            string elementoB,
            string terminalB)
        {
            return string.Equals(elementoA, elementoB, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(terminalA, terminalB, StringComparison.OrdinalIgnoreCase);
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
            {
                cabo.Vertices.Add(terminal.Posicao);
            }
            else if (cabo.Vertices.Count == 1)
            {
                cabo.Vertices.Add(terminal.Posicao);
            }
            else
            {
                cabo.Vertices[^1] = terminal.Posicao;
            }

            cabo.DefinirDestino(terminal.Posicao);
            return true;
        }

        private static Terminal? ObterTerminal(Elemento elemento, string terminalId)
        {
            if (elemento is not ITerminalOwner owner ||
                string.IsNullOrWhiteSpace(terminalId))
            {
                return null;
            }

            return owner.Terminais.FirstOrDefault(t =>
                string.Equals(t.Id, terminalId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
