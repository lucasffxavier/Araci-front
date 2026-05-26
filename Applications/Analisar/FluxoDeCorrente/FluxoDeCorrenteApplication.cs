using System;
using System.Threading.Tasks;
using System.Windows;
using Araci.DTOs;
using Araci.Services;

namespace Araci.Applications.Analisar.FluxoDeCorrente
{
    public class FluxoDeCorrenteApplication
    {
        private readonly EditorContext _context;

        public FluxoDeCorrenteApplication(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                Resultado = await _context.Simulation.ExecutarFluxoDeCorrenteAsync();

                if (options != null)
                    dssPath = SalvarArquivos(options, Resultado);

                MostrarResultado(Resultado, dssPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Fluxo de corrente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private string? SalvarArquivos(FluxoDeCorrenteOptions options, SimulationResultDto resultado)
        {
            try
            {
                SimulationExportPaths paths = _context.SimulationExport.GetPaths(options);

                if (_context.SimulationExport.Exists(paths) && !ConfirmarSubstituicao())
                    return null;

                return _context.SimulationExport.Save(options, resultado);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"A simula\u00E7\u00E3o foi aplicada, mas n\u00E3o foi poss\u00EDvel salvar os arquivos.\n\n{ex.Message}",
                    "Fluxo de corrente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return null;
            }
        }

        private static bool ConfirmarSubstituicao()
        {
            MessageBoxResult result = MessageBox.Show(
                "Um ou mais arquivos de sa\u00EDda j\u00E1 existem. Deseja substituir?",
                "Fluxo de corrente",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private void MostrarResultado(SimulationResultDto resultado, string? dssPath)
        {
            SimulationMessage message = _context.SimulationMessages.Build(resultado, dssPath);

            MessageBox.Show(message.Text, message.Title, MessageBoxButton.OK, message.Icon);
        }
    }
}
