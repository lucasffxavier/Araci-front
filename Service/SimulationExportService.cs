using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Araci.Applications.Analisar.FluxoDeCorrente;
using Araci.DTOs;

namespace Araci.Services
{
    public class SimulationExportService
    {
        public SimulationExportPaths GetPaths(FluxoDeCorrenteOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string baseName = SanitizarNomeArquivo(options.NomeArquivo);
            string dssFileName = baseName.EndsWith(".dss", StringComparison.OrdinalIgnoreCase)
                ? baseName
                : $"{baseName}.dss";

            string jsonBaseName = Path.GetFileNameWithoutExtension(dssFileName);
            string dssPath = Path.Combine(options.PastaSaida, dssFileName);
            string jsonPath = Path.Combine(options.PastaSaida, $"{jsonBaseName}_resultado.json");

            return new SimulationExportPaths(dssPath, jsonPath);
        }

        public bool Exists(SimulationExportPaths paths)
        {
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            return File.Exists(paths.DssPath) || File.Exists(paths.JsonPath);
        }

        public string Save(FluxoDeCorrenteOptions options, SimulationResultDto resultado)
        {
            if (resultado == null)
                throw new ArgumentNullException(nameof(resultado));

            SimulationExportPaths paths = GetPaths(options);
            Directory.CreateDirectory(options.PastaSaida);

            File.WriteAllText(paths.DssPath, resultado.Script, Encoding.UTF8);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(resultado, jsonOptions);
            File.WriteAllText(paths.JsonPath, json, Encoding.UTF8);

            return paths.DssPath;
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
    }

    public class SimulationExportPaths
    {
        public SimulationExportPaths(string dssPath, string jsonPath)
        {
            DssPath = dssPath;
            JsonPath = jsonPath;
        }

        public string DssPath { get; }

        public string JsonPath { get; }
    }
}
