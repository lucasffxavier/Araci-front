namespace Araci.DTOs
{
    public class LoadDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public string Barra { get; set; } = string.Empty;

        public int Fases { get; set; }

        public double R { get; set; }

        public double X { get; set; }

        public double PotenciaAtiva { get; set; }

        public double PotenciaReativa { get; set; }

        public string Conexao { get; set; } = string.Empty;

        public int Modelo { get; set; }
    }
}
