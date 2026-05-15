namespace Araci.Models
{
    public class Carga : Elemento
    {
        // =========================
        // INSTÂNCIA
        // =========================

        public string Barra { get; set; }

        public string Alimentador { get; set; }

        public double PotenciaAtivaKW { get; set; }

        public double PotenciaReativaKvar { get; set; }

        // =========================
        // TIPO
        // =========================

        public string ModeloCarga { get; set; }

        public string Conexao { get; set; }

        public double TensaoKV { get; set; }

        public int Fases { get; set; }

        public double FatorPotencia { get; set; }

        // =========================
        // CONSTRUTOR
        // =========================

        public Carga()
        {
            Nome = "LOAD-01";

            Barra = "BUS-03";
            Alimentador = "AL-01";

            PotenciaAtivaKW = 1500;
            PotenciaReativaKvar = 450;

            PosicaoX = 500;
            PosicaoY = 250;

            ModeloCarga = "Potência Constante";

            Conexao = "Wye";

            TensaoKV = 34.5;

            Fases = 3;

            FatorPotencia = 0.96;

            Categoria = "Cargas";
            Familia = "Cargas";
        }

        // =========================
        // CLONAGEM
        // =========================

        public override Elemento Clonar()
        {
            var clone = new Carga();

            CopiarBasePara(clone);

            clone.Barra = Barra;
            clone.Alimentador = Alimentador;

            clone.PotenciaAtivaKW = PotenciaAtivaKW;
            clone.PotenciaReativaKvar = PotenciaReativaKvar;

            clone.ModeloCarga = ModeloCarga;
            clone.Conexao = Conexao;

            clone.TensaoKV = TensaoKV;

            clone.Fases = Fases;

            clone.FatorPotencia = FatorPotencia;

            return clone;
        }
    }
}