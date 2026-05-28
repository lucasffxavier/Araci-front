using System.Windows;
using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Carga : ElementoEquipamento
    {
        public TipoCarga TipoCarga => (TipoCarga)Tipo!;

        public Carga()
        {
            Nome = "CARGA-001";
            Barra = "CARGA-001";
            Alimentador = 1;
            PotenciaAtiva = 800;
            PotenciaReativa = 300;
            CorrenteLinha = "0∠0°";
            CorrenteFaseA = "0∠0°";
            CorrenteFaseB = "0∠-120°";
            CorrenteFaseC = "0∠120°";
            TensaoLinha = "12.47∠0°";
            TensaoFaseA = "7.2∠0°";
            TensaoFaseB = "7.2∠-120°";
            TensaoFaseC = "7.2∠120°";

            PosicaoX = 500;
            PosicaoY = 250;
        }

        public void AtualizarTerminais(double largura)
        {
            AtualizarTerminais(largura, largura);
        }

        public void AtualizarTerminais(double largura, double altura)
        {
            var terminais = ObterTerminaisInternos();

            if (terminais.Count == 0)
                return;

            terminais[0].Barra = Barra;
            terminais[0].Direction = TerminalDirection.North;
            terminais[0].DefinirPosicaoLocal(
                new Point(largura / 2, 0),
                largura,
                altura);
        }

        public override Elemento Clonar()
        {
            var clone = new Carga();

            CopiarEquipamentoPara(clone);

            return clone;
        }
    }
}
