using System;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.Applications.Simulation;
using Araci.DTOs;
using Araci.Infrastructure.Simulation;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public class SimulationPipeline : ISimulationPipeline
    {
        private readonly CircuitDtoBuilder _circuitBuilder;
        private readonly ISimulationGateway _gateway;
        private readonly ISimulationResultApplier _simulationResults;

        public SimulationPipeline(EditorContext context)
            : this(
                new CircuitDtoBuilder(context?.Document ?? throw new ArgumentNullException(nameof(context))),
                new FastApiOpenDssGateway(),
                new SimulationResultApplier(
                    context.Document,
                    () => NotificarViewModels(context)))
        {
        }

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

        private static void NotificarViewModels(EditorContext context)
        {
            if (context.Viewport == null)
                return;

            foreach (ElementoViewModel vm in context.Viewport.Elementos)
            {
                if (vm.Modelo is Cabo or Carga)
                {
                    vm.NotificarPropriedades(
                        "CorrenteLinha",
                        "CorrenteFaseA",
                        "CorrenteFaseB",
                        "CorrenteFaseC");
                }
            }
        }
    }
}
