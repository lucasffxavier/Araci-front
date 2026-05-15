namespace Araci.Models.Tipos
{
    public class TipoCarga : TipoElemento
    {
        // =========================
        // ELÉTRICO
        // =========================

        public string ModeloCarga { get; set; }

        public string Conexao { get; set; }

        public double TensaoKV { get; set; }

        public int Fases { get; set; }

        public double FatorPotencia { get; set; }

        // =========================
        // CONSTRUTOR
        // =========================

        public TipoCarga()
        {
            NomeTipo = "Carga MT";

            Familia = "Cargas";
            Categoria = "Cargas";

            ModeloCarga = "Potência Constante";

            Conexao = "Wye";

            TensaoKV = 34.5;

            Fases = 3;

            FatorPotencia = 0.96;
        }
    }
}