namespace Araci.DTOs
{
    public class SlackDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public double Tensao { get; set; }

        public int Fases { get; set; }

        public string Barra { get; set; } = string.Empty;
    }
}
