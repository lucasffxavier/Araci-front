using System.Collections.Generic;
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

        private readonly List<Terminal> _terminais = new();

        public Barra()
        {
            Nome = "BARRA-001";

            DefinirParametro(new Parameter<double>(PARAM_ALTURA, ALTURA_PADRAO));
            DefinirParametro(new Parameter<string>(PARAM_TENSAO, "13,8∠0°"));

            CriarTerminais();
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
            if (_terminais.Count == 0)
                return;

            double centroX = largura / 2;
            double espacamento = Altura / (_terminais.Count - 1);

            for (int i = 0; i < _terminais.Count; i++)
            {
                _terminais[i].DefinirPosicaoLocal(
                    new Point(centroX, i * espacamento),
                    largura,
                    Altura);
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
            return value < ALTURA_MINIMA || double.IsNaN(value) || double.IsInfinity(value)
                ? ALTURA_MINIMA
                : value;
        }

        private void CriarTerminais()
        {
            _terminais.Clear();

            int quantidade = 24;

            for (int i = 0; i < quantidade; i++)
            {
                _terminais.Add(
                    new Terminal(
                        this,
                        new Point(),
                        $"BARRA-{i + 1:00}",
                        TerminalKind.Electrical,
                        TerminalDirection.East));
            }
        }
    }
}
