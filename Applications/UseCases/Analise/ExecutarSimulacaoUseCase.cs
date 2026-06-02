using System;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.Applications.Analisar.FluxoDeCorrente;
using Araci.DTOs;
using Araci.Services.Simulation;

namespace Araci.Applications.UseCases.Analise
{
    public class ExecutarSimulacaoUseCase
    {
        private readonly ISimulationPipeline _simulation;
        private readonly SimulationExportService _simulationExport;
        private readonly SimulationMessageBuilder _simulationMessages;
        private readonly IUserDialogService _dialogs;

        public ExecutarSimulacaoUseCase(
            ISimulationPipeline simulation,
            SimulationExportService simulationExport,
            SimulationMessageBuilder simulationMessages,
            IUserDialogService dialogs)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _simulationExport = simulationExport ?? throw new ArgumentNullException(nameof(simulationExport));
            _simulationMessages = simulationMessages ?? throw new ArgumentNullException(nameof(simulationMessages));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        }

        public SimulationResultDto Resultado { get; private set; } = new();

        public void ExecutarFluxoDeCorrente()
        {
            ExecutarFluxoDeCorrenteAsync().GetAwaiter().GetResult();
        }

        public Task ExecutarFluxoDeCorrenteAsync()
        {
            return ExecuteCoreAsync(null);
        }

        public Task ExecutarFluxoDeCorrenteAsync(FluxoDeCorrenteOptions options)
        {
            return ExecuteCoreAsync(options);
        }

        private async Task ExecuteCoreAsync(FluxoDeCorrenteOptions? options)
        {
            string? dssPath = null;

            try
            {
                Resultado = await _simulation.ExecutarFluxoDeCorrenteAsync();

                if (options != null)
                    dssPath = SalvarArquivos(options, Resultado);

                MostrarResultado(Resultado, dssPath);
            }
            catch (Exception ex)
            {
                _dialogs.ShowWarning("Fluxo de corrente", ex.Message);
            }
        }

        private string? SalvarArquivos(FluxoDeCorrenteOptions options, SimulationResultDto resultado)
        {
            try
            {
                SimulationExportPaths paths = _simulationExport.GetPaths(options);

                if (_simulationExport.Exists(paths) && !ConfirmarSubstituicao())
                    return null;

                return _simulationExport.Save(options, resultado);
            }
            catch (Exception ex)
            {
                _dialogs.ShowWarning(
                    "Fluxo de corrente",
                    $"A simulação foi aplicada, mas não foi possível salvar os arquivos.\n\n{ex.Message}");

                return null;
            }
        }

        private bool ConfirmarSubstituicao()
        {
            return _dialogs.Confirm(
                "Fluxo de corrente",
                "Um ou mais arquivos de saída já existem. Deseja substituir?");
        }

        private void MostrarResultado(SimulationResultDto resultado, string? dssPath)
        {
            SimulationMessage message = _simulationMessages.Build(resultado, dssPath);

            _dialogs.Show(message);
        }
    }
}
