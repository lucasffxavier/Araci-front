using System.Collections.Generic;

namespace Araci.DTOs
{
    public class SimulationResultDto
    {
        public bool Sucesso { get; set; }

        public string Script { get; set; } = string.Empty;

        public string Mensagem { get; set; } = string.Empty;

        public IList<string> Avisos { get; set; } = new List<string>();

        public IList<LineResultDto> Lines { get; set; } = new List<LineResultDto>();

        public IList<LoadResultDto> Loads { get; set; } = new List<LoadResultDto>();
    }

    public class LineResultDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public double Corrente { get; set; }

        public double? CorrenteLinha { get; set; }

        public double? CorrenteFaseA { get; set; }

        public double? AnguloFaseA { get; set; }

        public double? CorrenteFaseB { get; set; }

        public double? AnguloFaseB { get; set; }

        public double? CorrenteFaseC { get; set; }

        public double? AnguloFaseC { get; set; }
    }

    public class LoadResultDto
    {
        public string Id { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        public double Corrente { get; set; }

        public double? CorrenteLinha { get; set; }

        public double? CorrenteFaseA { get; set; }

        public double? AnguloFaseA { get; set; }

        public double? CorrenteFaseB { get; set; }

        public double? AnguloFaseB { get; set; }

        public double? CorrenteFaseC { get; set; }

        public double? AnguloFaseC { get; set; }
    }
}
