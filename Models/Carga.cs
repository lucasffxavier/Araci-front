using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Carga : ElementoEquipamento
    {
        public TipoCarga TipoCarga => (TipoCarga)Tipo!;

        public Carga()
        {
            Nome = "CARGA-01";
            Alimentador = "AL-01";
            PotenciaAtivaKW = 1500;
            PotenciaReativaKvar = 450;
            CorrenteLinha = "0 A";
            CorrenteFaseA = "0 A";
            CorrenteFaseB = "0 A";
            CorrenteFaseC = "0 A";
            TensaoLinha = "13.8 kV";
            TensaoFaseA = "7.97 kV";
            TensaoFaseB = "7.97 kV";
            TensaoFaseC = "7.97 kV";

            PosicaoX = 500;
            PosicaoY = 250;
        }

        public void AtualizarTerminais(double largura)
        {
            var terminais = ObterTerminaisInternos();

            if (terminais.Count == 0)
                return;

            terminais[0].Posicao = new Point(PosicaoX + largura / 2, PosicaoY);
        }

        public override Elemento Clonar()
        {
            var clone = new Carga();

            CopiarEquipamentoPara(clone);

            return clone;
        }
    }
}
