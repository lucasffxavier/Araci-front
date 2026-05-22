using Araci.Models.Tipos;

namespace Araci.Models.Tipos
{
    public class TipoBarra : TipoElemento
    {
        public double AlturaPadrao { get; set; } = 120;
        public int NumeroConexoes { get; set; } = 6;
    }
}