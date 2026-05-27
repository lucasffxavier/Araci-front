using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Sin : ElementoEquipamento
    {
        public TipoSin TipoSin => (TipoSin)Tipo!;

        public Sin()
        {
            Nome = "SIN-001";
            Barra = "SIN-001";
            Alimentador = 1;
            TensaoLinha = "12.47";
            TensaoFaseA = "7.2";
            TensaoFaseB = "7.2";
            TensaoFaseC = "7.2";
            CorrenteLinha = "0";
            CorrenteFaseA = "0";
            CorrenteFaseB = "0";
            CorrenteFaseC = "0";

            PosicaoX = 200;
            PosicaoY = 160;
        }

        public void AtualizarTerminais(double largura, double altura)
        {
            var terminais = ObterTerminaisInternos();

            if (terminais.Count == 0)
                return;

            terminais[0].Barra = Barra;
            terminais[0].Posicao = new Point(PosicaoX + largura / 2, PosicaoY + altura);
        }

        public override Elemento Clonar()
        {
            var clone = new Sin();

            CopiarEquipamentoPara(clone);

            return clone;
        }
    }
}
