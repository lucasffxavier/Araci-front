namespace Araci.Models
{
    public abstract class ElementoLinear
        : Elemento
    {
        public double PosicaoX2 { get; set; }

        public double PosicaoY2 { get; set; }

        public double Comprimento { get; set; }

        protected void CopiarLinearPara(
            ElementoLinear destino)
        {
            CopiarBasePara(destino);

            destino.PosicaoX2 = PosicaoX2;
            destino.PosicaoY2 = PosicaoY2;

            destino.Comprimento = Comprimento;
        }
    }
}