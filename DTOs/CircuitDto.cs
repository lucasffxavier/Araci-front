using System.Collections.Generic;

namespace Araci.DTOs
{
    public class CircuitDto
    {
        public IList<LoadDto> Loads { get; set; } = new List<LoadDto>();

        public IList<LineDto> Lines { get; set; } = new List<LineDto>();

        public IList<TransformerDto> Transformers { get; set; } = new List<TransformerDto>();

        public IList<GeneratorDto> Generators { get; set; } = new List<GeneratorDto>();

        public SlackDto Slack { get; set; } = new SlackDto();
    }
}
