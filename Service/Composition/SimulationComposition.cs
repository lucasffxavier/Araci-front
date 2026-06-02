using System;
using Araci.Applications.Simulation;
using Araci.Applications.UseCases.Analise;
using Araci.Core.Documents;
using Araci.Infrastructure.Simulation;
using Araci.Services.Simulation;
using Araci.Services.UI;

namespace Araci.Services.Composition
{
    internal static class SimulationComposition
    {
        public static SimulationComponents Create(
            AraciDocument document,
            Action notifySimulationResultViewModels,
            DialogService dialogs)
        {
            var results = new SimulationResultApplier(document, notifySimulationResultViewModels);
            var gateway = new FastApiOpenDssGateway();
            var circuitDtoBuilder = new CircuitDtoBuilder(document);
            var pipeline = new SimulationPipeline(circuitDtoBuilder, gateway, results);
            var export = new SimulationExportService();
            var messages = new SimulationMessageBuilder();
            var executarSimulacao = new ExecutarSimulacaoUseCase(pipeline, export, messages, dialogs);

            return new SimulationComponents(results, pipeline, export, messages, executarSimulacao);
        }
    }

    internal sealed record SimulationComponents(
        SimulationResultApplier Results,
        SimulationPipeline Pipeline,
        SimulationExportService Export,
        SimulationMessageBuilder Messages,
        ExecutarSimulacaoUseCase ExecutarSimulacao);
}
