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
            Alimentador = 1;
            PotenciaAtiva = 1500;
            PotenciaReativa = 450;
            CorrenteLinha = "0∠0°";
            CorrenteFaseA = "0∠0°";
            CorrenteFaseB = "0∠-120°";
            CorrenteFaseC = "0∠120°";
            TensaoLinha = "13.8∠0°";
            TensaoFaseA = "7.97∠0°";
            TensaoFaseB = "7.97∠-120°";
            TensaoFaseC = "7.97∠120°";

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
