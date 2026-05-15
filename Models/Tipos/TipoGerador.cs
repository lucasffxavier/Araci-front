namespace Araci.Models.Tipos
{
    public class TipoGerador : TipoElemento
    {
        // =========================
        // ELÉTRICO
        // =========================

        public string CategoriaGerador { get; set; }

        public string Fabricante { get; set; }

        public string Modelo { get; set; }

        public double PotenciaNominalKW { get; set; }

        public double TensaoKV { get; set; }

        public int Fases { get; set; }

        // =========================
        // CONSTRUTOR
        // =========================

        public TipoGerador()
        {
            NomeTipo = "Gerador Eólico";

            Familia = "Geradores";
            Categoria = "Geradores";

            CategoriaGerador = "Eólico";

            Fabricante = "WEG";

            Modelo = "WTG-5MW";

            PotenciaNominalKW = 5000;

            TensaoKV = 34.5;

            Fases = 3;
        }
    }
}