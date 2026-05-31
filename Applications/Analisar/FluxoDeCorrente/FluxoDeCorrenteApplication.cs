using System;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.DTOs;
using Araci.Services;

namespace Araci.Applications.Analisar.FluxoDeCorrente
{
    public class FluxoDeCorrenteApplication
    {
        private readonly ISimulationPipeline _simulation;
        private readonly SimulationExportService _simulationExport;
        private readonly SimulationMessageBuilder _simulationMessages;
        private readonly IUserDialogService _dialogs;

        public FluxoDeCorrenteApplication(
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

        public void Executar()
        {
            ExecutarAsync().GetAwaiter().GetResult();
        }

        public Task ExecutarAsync()
        {
            return ExecuteCoreAsync(null);
        }

        public async Task ExecutarAsync(FluxoDeCorrenteOptions options)
        {
            await ExecuteCoreAsync(options);
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
                    $"A simula\u00E7\u00E3o foi aplicada, mas n\u00E3o foi poss\u00EDvel salvar os arquivos.\n\n{ex.Message}");

                return null;
            }
        }

        private bool ConfirmarSubstituicao()
        {
            return _dialogs.Confirm(
                "Fluxo de corrente",
                "Um ou mais arquivos de sa\u00EDda j\u00E1 existem. Deseja substituir?");
        }

        private void MostrarResultado(SimulationResultDto resultado, string? dssPath)
        {
            SimulationMessage message = _simulationMessages.Build(resultado, dssPath);

            _dialogs.Show(message);
        }
    }
}
