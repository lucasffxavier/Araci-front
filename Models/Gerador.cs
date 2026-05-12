namespace Araci.Models
{
    public class Gerador : Elemento
    {

        // =========================
        // PARÂMETROS DE INSTÂNCIA
        // =========================

        public string Barra { get; set; }

        public string Alimentador { get; set; }

        public double PotenciaAtivaKW { get; set; }

        public double FatorPotencia { get; set; }

        // =========================
        // PARÂMETROS DE TIPO
        // =========================

        public string TipoGerador { get; set; }

        public string Fabricante { get; set; }

        public string Modelo { get; set; }

        public double PotenciaNominalKW { get; set; }

        public double TensaoKV { get; set; }

        public int Fases { get; set; }

        public Gerador()
        {
            Nome = "GER-01";
            Barra = "BUS-01";
            Alimentador = "AL-01";
            PotenciaAtivaKW = 5000;
            FatorPotencia = 0.98;
            PosicaoX = 300;
            PosicaoY = 200;
            TipoGerador = "Eólico";
            Fabricante = "WEG";
            Modelo = "WTG-5MW";
            PotenciaNominalKW = 5000;
            TensaoKV = 34.5;
            Fases = 3;
            Categoria = "Geradores";
            Familia = "Geradores";
        }




    }
}