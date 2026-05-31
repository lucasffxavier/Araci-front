using System;
using Araci.Core.Documents;
using Araci.DTOs;

namespace Araci.Applications.Simulation
{
    public class CircuitDtoBuilder
    {
        private readonly AraciDocument _document;

        public CircuitDtoBuilder(AraciDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public CircuitDto Build()
        {
            ParameterReader reader = new(_document);
            CircuitBuilder builder = new(reader);
            return builder.Build();
        }
    }
}
