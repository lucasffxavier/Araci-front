using System;
using Araci.Applications.Simulation;
using Araci.Core.Documents;
using Araci.Infrastructure.Simulation;
using Araci.Services.Simulation;

namespace Araci.Services.Composition
{
    internal static class SimulationComposition
    {
        public static SimulationComponents Create(
            AraciDocument document,
            Action notifySimulationResultViewModels)
        {
            var results = new SimulationResultApplier(document, notifySimulationResultViewModels);
            var gateway = new FastApiOpenDssGateway();
            var circuitDtoBuilder = new CircuitDtoBuilder(document);
            var pipeline = new SimulationPipeline(circuitDtoBuilder, gateway, results);
            var export = new SimulationExportService();
            var messages = new SimulationMessageBuilder();

            return new SimulationComponents(results, pipeline, export, messages);
        }
    }

    internal sealed record SimulationComponents(
        SimulationResultApplier Results,
        SimulationPipeline Pipeline,
        SimulationExportService Export,
        SimulationMessageBuilder Messages);
}
