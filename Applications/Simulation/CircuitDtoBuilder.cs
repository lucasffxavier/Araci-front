using System;
using Araci.DTOs;
using Araci.Services;

namespace Araci.Applications.Simulation
{
    public class CircuitDtoBuilder
    {
        private readonly EditorContext _context;

        public CircuitDtoBuilder(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public CircuitDto Build()
        {
            ParameterReader reader = new(_context);
            CircuitBuilder builder = new(reader);
            return builder.Build();
        }
    }
}
