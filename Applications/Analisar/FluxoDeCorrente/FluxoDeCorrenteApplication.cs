using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                CoreApi api = new(_context);
                ParameterReader reader = new(api);
                CircuitBuilder builder = new(reader);
                CircuitDto dto = builder.Build();
                SimulationApiClient client = new();

                Resultado = await client.SimularTipadoAsync(dto);

                AplicarResultado(api, Resultado);
                NotificarViewModels();

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
            cabo.CorrenteFaseA = FormatPolar(resultado.CorrenteFaseA ?? corrente, resultado.AnguloFaseA ?? 0);
            cabo.CorrenteFaseB = FormatPolar(resultado.CorrenteFaseB ?? corrente, resultado.AnguloFaseB ?? -120);
            cabo.CorrenteFaseC = FormatPolar(resultado.CorrenteFaseC ?? corrente, resultado.AnguloFaseC ?? 120);
        }

        private static void AplicarCorrentes(Carga carga, LoadResultDto resultado)
        {
            double corrente = resultado.Corrente;

            carga.CorrenteLinha = FormatPolar(resultado.CorrenteLinha ?? corrente, 0);
            carga.CorrenteFaseA = FormatPolar(resultado.CorrenteFaseA ?? corrente, resultado.AnguloFaseA ?? 0);
            carga.CorrenteFaseB = FormatPolar(resultado.CorrenteFaseB ?? corrente, resultado.AnguloFaseB ?? -120);
            carga.CorrenteFaseC = FormatPolar(resultado.CorrenteFaseC ?? corrente, resultado.AnguloFaseC ?? 120);
        }

        private static string FormatPolar(double magnitude, double angle)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.##}∠{1:0.##}°",
                magnitude,
                angle);
        }

        private static string? SalvarArquivos(FluxoDeCorrenteOptions options, SimulationResultDto resultado)
        {
            try
            {
                Directory.CreateDirectory(options.PastaSaida);

                string baseName = SanitizarNomeArquivo(options.NomeArquivo);
                string dssFileName = baseName.EndsWith(".dss", StringComparison.OrdinalIgnoreCase)
                    ? baseName
                    : $"{baseName}.dss";

                string jsonBaseName = Path.GetFileNameWithoutExtension(dssFileName);
                string dssPath = Path.Combine(options.PastaSaida, dssFileName);
                string jsonPath = Path.Combine(options.PastaSaida, $"{jsonBaseName}_resultado.json");

                if ((File.Exists(dssPath) || File.Exists(jsonPath)) && !ConfirmarSubstituicao())
                    return null;

                File.WriteAllText(dssPath, resultado.Script, Encoding.UTF8);

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(resultado, jsonOptions);
                File.WriteAllText(jsonPath, json, Encoding.UTF8);

                return dssPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"A simulação foi aplicada, mas não foi possível salvar os arquivos.\n\n{ex.Message}",
                    "Fluxo de corrente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return null;
            }
        }

        private static bool ConfirmarSubstituicao()
        {
            MessageBoxResult result = MessageBox.Show(
                "Um ou mais arquivos de saída já existem. Deseja substituir?",
                "Fluxo de corrente",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private static string SanitizarNomeArquivo(string nomeArquivo)
        {
            string nome = string.IsNullOrWhiteSpace(nomeArquivo)
                ? "circuito"
                : nomeArquivo.Trim();

            foreach (char invalid in Path.GetInvalidFileNameChars())
                nome = nome.Replace(invalid.ToString(), string.Empty);

            return string.IsNullOrWhiteSpace(nome) ? "circuito" : nome;
        }

        private static void MostrarResultado(SimulationResultDto resultado, string? dssPath)
        {
            var message = new StringBuilder();

            message.AppendLine(resultado.Sucesso ? "Fluxo resolvido com sucesso." : "Fluxo retornou falha.");

            if (!string.IsNullOrWhiteSpace(resultado.Mensagem))
                message.AppendLine(resultado.Mensagem);

            if (!string.IsNullOrWhiteSpace(dssPath))
            {
                message.AppendLine();
                message.AppendLine("Arquivo DSS salvo em:");
                message.AppendLine(dssPath);
            }

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
