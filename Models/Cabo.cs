using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Cabo
        : ElementoLinear
    {
        public string BarraOrigem { get; set; }

        public string BarraDestino { get; set; }

        public TipoCabo TipoCabo =>
            (TipoCabo)Tipo!;

        public Cabo()
        {
            Nome = "CB-01";

            BarraOrigem = "BUS-01";
            BarraDestino = "BUS-02";

            Comprimento = 120;

            PosicaoX = 100;
            PosicaoY = 100;

            PosicaoX2 = 400;
            PosicaoY2 = 100;
        }

        public override Elemento Clonar()
        {
            var clone = new Cabo();

            CopiarLinearPara(clone);

            clone.BarraOrigem = BarraOrigem;
            clone.BarraDestino = BarraDestino;

            return clone;
        }
    }
}