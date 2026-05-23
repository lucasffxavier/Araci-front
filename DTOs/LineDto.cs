namespace Araci.DTOs
{
    public class LineDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public string Barra1 { get; set; } = string.Empty;

        public string Barra2 { get; set; } = string.Empty;

        public int Fases { get; set; }

        public double Comprimento { get; set; }

        public double R1 { get; set; }

        public double X1 { get; set; }

        public double R0 { get; set; }

        public double X0 { get; set; }
    }
}
