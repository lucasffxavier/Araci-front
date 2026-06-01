using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Araci.Core.Rendering;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Barra : Elemento, ITerminalOwner
    {
        public const string PARAM_ALTURA = "Altura";
        public const string PARAM_TENSAO = "Tensao";
        public const double ALTURA_PADRAO = 120;
        public const double ALTURA_MINIMA = 40;
        private const int TERMINAIS_PADRAO = 24;
        private const double ESPACAMENTO_TERMINAIS = ALTURA_PADRAO / (TERMINAIS_PADRAO - 1);
        private readonly List<Terminal> _terminais = new();

        public Barra()
        {
            Nome = "BARRA-001";
            DefinirParametro(new Parameter<double>(PARAM_ALTURA, ALTURA_PADRAO));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO, "13,8∠0°"));
            AtualizarTerminais();
        }

        public IReadOnlyList<Terminal> Terminais => _terminais;
        public override ElementoDomainRole DomainRole => ElementoDomainRole.EletricoTopologico;
        public TipoBarra TipoBarra => (TipoBarra)Tipo!;

        public double Altura
        {
            get => NormalizarAltura(Obter<double>(PARAM_ALTURA));
            set => Definir(PARAM_ALTURA, NormalizarAltura(value));
        }

        public string Tensao
        {
            get => Obter<string>(PARAM_TENSAO);
            set => Definir(PARAM_TENSAO, value);
        }

        public void AtualizarTerminais()
        {
            AtualizarTerminais(ElementGeometryDefaults.BarraLargura);
        }

        public void AtualizarTerminais(double largura)
        {
            AtualizarTerminais(largura, null);
        }

        public void AtualizarTerminais(double largura, IReadOnlySet<string>? terminaisProtegidos)
        {
            int quantidade = CalcularQuantidadeTerminais(Altura);
            AjustarQuantidadeTerminais(quantidade, terminaisProtegidos);
            double centroX = largura / 2;
            OrdenarTerminaisPorSlot();

            for (int i = 0; i < _terminais.Count; i++)
            {
                double y = CalcularYTerminal(ObterSlotTerminal(_terminais[i], i), Altura);
                _terminais[i].DefinirPosicaoLocal(new Point(centroX, y), largura, Altura);
            }
        }

        public override Elemento Clonar()
        {
            var clone = new Barra();
            CopiarBasePara(clone);
            clone.Altura = Altura;
            clone.Tensao = Tensao;
            clone.AtualizarTerminais();
            return clone;
        }

        public static double NormalizarAltura(double value)
        {
            return value < ALTURA_MINIMA || double.IsNaN(value) || double.IsInfinity(value) ? ALTURA_MINIMA : value;
        }

        private static int CalcularQuantidadeTerminais(double altura)
        {
            altura = NormalizarAltura(altura);
            return Math.Max(2, (int)Math.Floor(altura / ESPACAMENTO_TERMINAIS) + 1);
        }

        private static double CalcularYTerminal(int index, double altura)
        {
            double y = index * ESPACAMENTO_TERMINAIS;
            return y > altura ? altura : y;
        }

        private void AjustarQuantidadeTerminais(int quantidade, IReadOnlySet<string>? terminaisProtegidos)
        {
            var protegidos = terminaisProtegidos == null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(terminaisProtegidos, StringComparer.OrdinalIgnoreCase);

            for (int i = _terminais.Count - 1; i >= 0; i--)
            {
                Terminal terminal = _terminais[i];
                int slot = ObterSlotTerminal(terminal, i);

                if (slot >= quantidade && !protegidos.Contains(terminal.Id))
                    _terminais.RemoveAt(i);
            }

            var slotsExistentes = new HashSet<int>();

            for (int i = 0; i < _terminais.Count; i++)
                slotsExistentes.Add(ObterSlotTerminal(_terminais[i], i));

            for (int slot = 0; slot < quantidade; slot++)
            {
                if (slotsExistentes.Contains(slot))
                    continue;

                _terminais.Add(new Terminal(this, new Point(), CriarTerminalId(slot), TerminalKind.Electrical, TerminalDirection.East));
                slotsExistentes.Add(slot);
            }
        }

        private void OrdenarTerminaisPorSlot()
        {
            var indicesOriginais = new Dictionary<Terminal, int>();

            for (int i = 0; i < _terminais.Count; i++)
                indicesOriginais[_terminais[i]] = i;

            _terminais.Sort((a, b) =>
                ObterSlotTerminal(a, indicesOriginais[a]).CompareTo(ObterSlotTerminal(b, indicesOriginais[b])));
        }

        private static int ObterSlotTerminal(Terminal terminal, int fallback)
        {
            const string prefixo = "BARRA-";

            if (terminal.Id.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(terminal.Id[prefixo.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out int numero) &&
                numero > 0)
            {
                return numero - 1;
            }

            return fallback;
        }

        private static string CriarTerminalId(int slot)
        {
            return $"BARRA-{slot + 1:00}";
        }
    }
}
