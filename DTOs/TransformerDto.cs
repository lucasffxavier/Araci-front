namespace Araci.DTOs
{
    public class TransformerDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public int Fases { get; set; }

        public int Enrolamentos { get; set; }

        public string BarraPrimario { get; set; } = string.Empty;

        public string BarraSecundario { get; set; } = string.Empty;

        public double TensaoPrimarioKV { get; set; }

        public double TensaoSecundarioKV { get; set; }

        public double PotenciaKVA { get; set; }

        public double RPercentual { get; set; }

        public double XPercentual { get; set; }

        public string LigacaoPrimario { get; set; } = string.Empty;

        public string LigacaoSecundario { get; set; } = string.Empty;
    }
}
