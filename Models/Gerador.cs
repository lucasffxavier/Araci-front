using Araci.Models.Tipos;

namespace Araci.Models
{
    public class Gerador
        : ElementoEquipamento
    {
        public double FatorPotencia { get; set; }

        public TipoGerador TipoGerador =>
            (TipoGerador)Tipo!;

        public Gerador()
        {
            Nome = "GER-01";

            Barra = "BUS-01";

            Alimentador = "AL-01";

            PotenciaAtivaKW = 5000;

            FatorPotencia = 0.98;

            PosicaoX = 300;
            PosicaoY = 200;
        }

        public override Elemento Clonar()
        {
            var clone = new Gerador();

            CopiarEquipamentoPara(clone);

            clone.FatorPotencia =
                FatorPotencia;

            return clone;
        }
    }
}