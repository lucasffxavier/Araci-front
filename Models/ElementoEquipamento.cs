using Araci.Models.Tipos;

namespace Araci.Models
{
    public abstract class ElementoEquipamento
        : Elemento
    {
        public string Barra { get; set; }
            = string.Empty;

        public string Alimentador { get; set; }
            = string.Empty;

        public double PotenciaAtivaKW { get; set; }

        protected void CopiarEquipamentoPara(
            ElementoEquipamento destino)
        {
            CopiarBasePara(destino);

            destino.Barra = Barra;
            destino.Alimentador = Alimentador;
            destino.PotenciaAtivaKW = PotenciaAtivaKW;
        }
    }
}