using System.Collections.Generic;

namespace Araci.DTOs
{
    public class SimulationResultDto
    {
        public IList<LineResultDto> Lines { get; set; } = new List<LineResultDto>();

        public IList<LoadResultDto> Loads { get; set; } = new List<LoadResultDto>();
    }

    public class LineResultDto
    {
        public string Id { get; set; } = string.Empty;

        public double Corrente { get; set; }
    }

    public class LoadResultDto
    {
        public string Id { get; set; } = string.Empty;

        public double Corrente { get; set; }
    }
}
