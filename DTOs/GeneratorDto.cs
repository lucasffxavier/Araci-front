namespace Araci.DTOs
{
    public class GeneratorDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public string Barra { get; set; } = string.Empty;

        public int Fases { get; set; }

        public double Tensao { get; set; }

        public double Potencia { get; set; }

        public double FP { get; set; }
    }
}
