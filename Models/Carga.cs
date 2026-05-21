using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Carga : ElementoEquipamento
    {
        public const string PARAM_POTENCIA_REATIVA = "PotenciaReativaKvar";

        public TipoCarga TipoCarga => (TipoCarga)Tipo!;

        public double PotenciaReativaKvar
        {
            get => Obter<double>(PARAM_POTENCIA_REATIVA);
            set => Definir(PARAM_POTENCIA_REATIVA, value);
        }

        public Carga()
        {
            Nome = "LOAD-01";
            Barra = "BUS-03";
            Alimentador = "AL-01";
            PotenciaAtivaKW = 1500;

            DefinirParametro(
                new Parameter<double>(
                    PARAM_POTENCIA_REATIVA,
                    450));

            PosicaoX = 500;
            PosicaoY = 250;
        }

        public void AtualizarTerminais(double largura)
        {
            var terminais = ObterTerminaisInternos();

            if (terminais.Count == 0)
                return;

            terminais[0].Posicao = new Point(
                PosicaoX + largura / 2,
                PosicaoY);
        }

        public override Elemento Clonar()
        {
            var clone = new Carga();
            CopiarEquipamentoPara(clone);
            return clone;
        }
    }
}