using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.Applications.UseCases.Analise;
using Araci.DTOs;
using Araci.Services.Simulation;

namespace Araci.Applications.Analisar.FluxoDeCorrente
{
    public class FluxoDeCorrenteApplication
    {
        private readonly ExecutarSimulacaoUseCase _useCase;

        public FluxoDeCorrenteApplication(
            ISimulationPipeline simulation,
            SimulationExportService simulationExport,
            SimulationMessageBuilder simulationMessages,
            IUserDialogService dialogs)
            : this(new ExecutarSimulacaoUseCase(simulation, simulationExport, simulationMessages, dialogs))
        {
        }

        public FluxoDeCorrenteApplication(ExecutarSimulacaoUseCase useCase)
        {
            _useCase = useCase ?? throw new System.ArgumentNullException(nameof(useCase));
        }

        public SimulationResultDto Resultado => _useCase.Resultado;

        public void Executar()
        {
            _useCase.ExecutarFluxoDeCorrente();
        }

        public Task ExecutarAsync()
        {
            return _useCase.ExecutarFluxoDeCorrenteAsync();
        }

        public Task ExecutarAsync(FluxoDeCorrenteOptions options)
        {
            return _useCase.ExecutarFluxoDeCorrenteAsync(options);
        }
    }
}
