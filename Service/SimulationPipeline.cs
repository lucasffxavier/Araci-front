using System;
using System.Threading.Tasks;
using Araci.DTOs;

namespace Araci.Services
{
    public class SimulationPipeline
    {
        private readonly EditorContext _context;

        public SimulationPipeline(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<SimulationResultDto> ExecutarFluxoDeCorrenteAsync()
        {
            ParameterReader reader = new(_context);
            CircuitBuilder builder = new(reader);
            CircuitDto dto = builder.Build();
            SimulationApiClient client = new();

            SimulationResultDto resultado = await client.SimularTipadoAsync(dto);
            _context.SimulationResults.Apply(resultado);

            return resultado;
        }
    }
}
