// Models/Barra.cs

using System.Collections.Generic;
using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Barra : Elemento, ITerminalOwner
    {
        public const string PARAM_ALTURA = "Altura";

        private readonly List<Terminal> _terminais = new();

        public IReadOnlyList<Terminal> Terminais =>
            _terminais;

        public TipoBarra TipoBarra =>
            (TipoBarra)Tipo!;

        public double Altura
        {
            get => Obter<double>(PARAM_ALTURA);
            set => Definir(PARAM_ALTURA, value);
        }

        public Barra()
        {
            Nome = "BARRA-001";

            DefinirParametro(
                new Parameter<double>(
                    PARAM_ALTURA,
                    120));

            CriarTerminais();

            AtualizarTerminais();
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
                        new Point()));
            }
        }

        public void AtualizarTerminais()
        {
            if (_terminais.Count == 0)
                return;

            double centroX =
                PosicaoX + 5;

            double espacamento =
                Altura / (_terminais.Count - 1);

            for (int i = 0; i < _terminais.Count; i++)
            {
                _terminais[i].Posicao =
                    new Point(
                        centroX,
                        PosicaoY + i * espacamento);
            }
        }

        public override Elemento Clonar()
        {
            var clone = new Barra();

            CopiarBasePara(clone);

            clone.Altura = Altura;

            clone.AtualizarTerminais();

            return clone;
        }
    }
}