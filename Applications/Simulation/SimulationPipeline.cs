using System;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.DTOs;

namespace Araci.Applications.Simulation
{
    public class SimulationPipeline : ISimulationPipeline
    {
        private readonly CircuitDtoBuilder _circuitBuilder;
        private readonly ISimulationGateway _gateway;
        private readonly ISimulationResultApplier _simulationResults;

        public SimulationPipeline(
            CircuitDtoBuilder circuitBuilder,
            ISimulationGateway gateway,
            ISimulationResultApplier simulationResults)
        {
            _circuitBuilder = circuitBuilder ?? throw new ArgumentNullException(nameof(circuitBuilder));
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            _simulationResults = simulationResults ?? throw new ArgumentNullException(nameof(simulationResults));
        }

        public async Task<SimulationResultDto> ExecutarFluxoDeCorrenteAsync()
        {
            CircuitDto dto = _circuitBuilder.Build();
            SimulationResultDto resultado = await _gateway.SimularAsync(dto);
            _simulationResults.Apply(resultado);

            return resultado;
        }
    }
}
