using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Araci.API;
using Araci.DTOs;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

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

        public async Task ExecutarAsync()
        {
            try
            {
                CoreApi api = new(_context);
                ParameterReader reader = new(api);
                CircuitBuilder builder = new(reader);
                CircuitDto dto = builder.Build();
                SimulationApiClient client = new();

                Resultado = await client.SimularTipadoAsync(dto);

                AplicarResultado(api, Resultado);
                NotificarViewModels();
                MostrarResultado(Resultado);
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

        private static void AplicarResultado(CoreApi api, SimulationResultDto resultado)
        {
            foreach (LineResultDto lineResult in resultado.Lines)
            {
                Cabo? cabo = api.ObterElementos<Cabo>()
                    .FirstOrDefault(c => string.Equals(c.Id.ToString(), lineResult.Id, StringComparison.OrdinalIgnoreCase));

                if (cabo != null)
                    AplicarCorrentes(cabo, lineResult);
            }

            foreach (LoadResultDto loadResult in resultado.Loads)
            {
                Carga? carga = api.ObterElementos<Carga>()
                    .FirstOrDefault(c => string.Equals(c.Id.ToString(), loadResult.Id, StringComparison.OrdinalIgnoreCase));

                if (carga != null)
                    AplicarCorrentes(carga, loadResult);
            }
        }

        private void NotificarViewModels()
        {
            if (_context.Viewport == null)
                return;

            foreach (ElementoViewModel vm in _context.Viewport.Elementos)
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

        private static void AplicarCorrentes(Cabo cabo, LineResultDto resultado)
        {
            double corrente = resultado.Corrente;

            cabo.CorrenteLinha = FormatPolar(resultado.CorrenteLinha ?? corrente, 0);
            cabo.CorrenteFaseA = FormatPolar(resultado.CorrenteFaseA ?? corrente, 0);
            cabo.CorrenteFaseB = FormatPolar(resultado.CorrenteFaseB ?? corrente, -120);
            cabo.CorrenteFaseC = FormatPolar(resultado.CorrenteFaseC ?? corrente, 120);
        }

        private static void AplicarCorrentes(Carga carga, LoadResultDto resultado)
        {
            double corrente = resultado.Corrente;

            carga.CorrenteLinha = FormatPolar(resultado.CorrenteLinha ?? corrente, 0);
            carga.CorrenteFaseA = FormatPolar(resultado.CorrenteFaseA ?? corrente, 0);
            carga.CorrenteFaseB = FormatPolar(resultado.CorrenteFaseB ?? corrente, -120);
            carga.CorrenteFaseC = FormatPolar(resultado.CorrenteFaseC ?? corrente, 120);
        }

        private static string FormatPolar(double magnitude, double angle)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.##}∠{1}°",
                magnitude,
                angle);
        }

        private static void MostrarResultado(SimulationResultDto resultado)
        {
            var message = new StringBuilder();

            message.AppendLine(resultado.Sucesso ? "Fluxo resolvido com sucesso." : "Fluxo retornou falha.");

            if (!string.IsNullOrWhiteSpace(resultado.Mensagem))
                message.AppendLine(resultado.Mensagem);

            if (resultado.Avisos.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("Avisos:");

                foreach (string aviso in resultado.Avisos)
                    message.AppendLine($"- {aviso}");
            }

            if (!string.IsNullOrWhiteSpace(resultado.Script))
            {
                message.AppendLine();
                message.AppendLine("Script DSS gerado:");
                message.AppendLine(resultado.Script);
            }

            MessageBox.Show(
                message.ToString(),
                "Fluxo de corrente",
                MessageBoxButton.OK,
                resultado.Sucesso ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }
}
