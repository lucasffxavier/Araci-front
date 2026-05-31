using System;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.DTOs;

namespace Araci.Services
{
    public class SimulationPipeline : ISimulationPipeline
    {
        private readonly EditorContext _context;
        private readonly ISimulationResultApplier _simulationResults;

        public SimulationPipeline(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _simulationResults = context.SimulationResults;
        }

        public async Task<SimulationResultDto> ExecutarFluxoDeCorrenteAsync()
        {
            ParameterReader reader = new(_context);
            CircuitBuilder builder = new(reader);
            CircuitDto dto = builder.Build();
            SimulationApiClient client = new();

            SimulationResultDto resultado = await client.SimularTipadoAsync(dto);
            _simulationResults.Apply(resultado);

            return resultado;
        }
    }
}
