using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Araci.DTOs
{
    public class SimulationApiClient
    {
        private const string DefaultSimulationUrl = "http://127.0.0.1:8000/simular";
        private const string ScriptDebugPath = @"C:\Temp\araci_script_debug.txt";

        private static readonly JsonSerializerOptions RequestJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly string _simulationUrl;

        public SimulationApiClient()
            : this(CreateDefaultHttpClient(), DefaultSimulationUrl)
        {
        }

        public SimulationApiClient(string simulationUrl)
            : this(CreateDefaultHttpClient(), simulationUrl)
        {
        }

        public SimulationApiClient(HttpClient httpClient)
            : this(httpClient, DefaultSimulationUrl)
        {
        }

        public SimulationApiClient(HttpClient httpClient, string simulationUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _simulationUrl = string.IsNullOrWhiteSpace(simulationUrl)
                ? DefaultSimulationUrl
                : simulationUrl;
        }

        private static HttpClient CreateDefaultHttpClient()
        {
            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<string> SimularAsync(CircuitDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                string json = JsonSerializer.Serialize(dto, RequestJsonOptions);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await _httpClient.PostAsync(_simulationUrl, content);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Erro ao simular circuito. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Resposta: {responseBody}");
                }

                return responseBody;
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException(
                    "Tempo limite excedido ao chamar a API de simulacao.",
                    ex);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Erro ao enviar circuito para a API de simulacao.",
                    ex);
            }
        }

        public async Task<SimulationResultDto> SimularTipadoAsync(CircuitDto dto)
        {
            string responseBody = await SimularAsync(dto);
            return DeserializeResult(responseBody, dto);
        }

        private static SimulationResultDto DeserializeResult(string responseBody, CircuitDto dto)
        {
            var result = new SimulationResultDto();

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            result.Sucesso = ReadStatus(root);
            result.Script = ReadString(root, "script", "dss_script");
            WriteScriptDebug(result.Script);
            result.Mensagem = ReadString(root, "mensagem", "message", "erro", "error");
            result.Avisos = ReadStringArray(root, "avisos", "warnings", "mensagens", "messages");

            JsonElement resultRoot = root;

            if (TryGetProperty(root, out JsonElement resultado, "resultado", "result"))
            {
                if (string.IsNullOrWhiteSpace(result.Mensagem))
                    result.Mensagem = ReadString(resultado, "mensagem", "message", "erro", "error");

                foreach (string aviso in ReadStringArray(resultado, "avisos", "warnings", "mensagens", "messages"))
                    result.Avisos.Add(aviso);

                resultRoot = resultado;
            }

            bool hasLines = ReadLineResults(root, dto, result);
            bool hasLoads = ReadLoadResults(root, dto, result);

            if (!hasLines && resultRoot.ValueKind != JsonValueKind.Undefined)
                hasLines = ReadLineResults(resultRoot, dto, result);

            if (!hasLoads && resultRoot.ValueKind != JsonValueKind.Undefined)
                hasLoads = ReadLoadResults(resultRoot, dto, result);

            if (!hasLines || !hasLoads)
                ReadElementResults(resultRoot, dto, result, includeLines: !hasLines, includeLoads: !hasLoads);

            return result;
        }

        private static void WriteScriptDebug(string script)
        {
            string? directory = Path.GetDirectoryName(ScriptDebugPath);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(ScriptDebugPath, script);
        }

        private static bool ReadStatus(JsonElement root)
        {
            string status = ReadString(root, "status");

            if (!string.IsNullOrWhiteSpace(status))
                return string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);

            if (TryGetProperty(root, out JsonElement success, "sucesso", "success", "converged") &&
                success.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                return success.GetBoolean();
            }

            return false;
        }

        private static bool ReadLineResults(JsonElement root, CircuitDto dto, SimulationResultDto result)
        {
            if (!TryGetProperty(root, out JsonElement linhas, "lines", "linhas", "lineResults", "line_results") ||
                linhas.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement line in linhas.EnumerateArray())
            {
                string key = ReadString(line, "id", "lineId", "line_id", "linha", "cabo", "nome", "name");
                double corrente = ReadDouble(line, "corrente", "current", "i");

                result.Lines.Add(new LineResultDto
                {
                    Id = ResolveLineId(dto, key),
                    Nome = ReadString(line, "nome", "name"),
                    Corrente = corrente,
                    CorrenteLinha = ReadNullableDouble(line, "correnteLinha", "corrente_linha", "lineCurrent", "line_current") ?? corrente,
                    CorrenteFaseA = ReadNullableDouble(line, "correnteFaseA", "corrente_fase_a", "phaseACurrent", "phase_a_current"),
                    AnguloFaseA = ReadNullableDouble(line, "anguloFaseA", "angulo_fase_a", "phaseAAngle", "phase_a_angle"),
                    CorrenteFaseB = ReadNullableDouble(line, "correnteFaseB", "corrente_fase_b", "phaseBCurrent", "phase_b_current"),
                    AnguloFaseB = ReadNullableDouble(line, "anguloFaseB", "angulo_fase_b", "phaseBAngle", "phase_b_angle"),
                    CorrenteFaseC = ReadNullableDouble(line, "correnteFaseC", "corrente_fase_c", "phaseCCurrent", "phase_c_current"),
                    AnguloFaseC = ReadNullableDouble(line, "anguloFaseC", "angulo_fase_c", "phaseCAngle", "phase_c_angle")
                });
            }

            return true;
        }

        private static bool ReadLoadResults(JsonElement root, CircuitDto dto, SimulationResultDto result)
        {
            if (!TryGetProperty(root, out JsonElement cargas, "loads", "cargas", "loadResults", "load_results") ||
                cargas.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement load in cargas.EnumerateArray())
            {
                string key = ReadString(load, "id", "loadId", "load_id", "carga", "nome", "name");
                double corrente = ReadDouble(load, "corrente", "current", "i");

                result.Loads.Add(new LoadResultDto
                {
                    Id = ResolveLoadId(dto, key),
                    Nome = ReadString(load, "nome", "name"),
                    Corrente = corrente,
                    CorrenteLinha = ReadNullableDouble(load, "correnteLinha", "corrente_linha", "lineCurrent", "line_current") ?? corrente,
                    CorrenteFaseA = ReadNullableDouble(load, "correnteFaseA", "corrente_fase_a", "phaseACurrent", "phase_a_current"),
                    AnguloFaseA = ReadNullableDouble(load, "anguloFaseA", "angulo_fase_a", "phaseAAngle", "phase_a_angle"),
                    CorrenteFaseB = ReadNullableDouble(load, "correnteFaseB", "corrente_fase_b", "phaseBCurrent", "phase_b_current"),
                    AnguloFaseB = ReadNullableDouble(load, "anguloFaseB", "angulo_fase_b", "phaseBAngle", "phase_b_angle"),
                    CorrenteFaseC = ReadNullableDouble(load, "correnteFaseC", "corrente_fase_c", "phaseCCurrent", "phase_c_current"),
                    AnguloFaseC = ReadNullableDouble(load, "anguloFaseC", "angulo_fase_c", "phaseCAngle", "phase_c_angle")
                });
            }

            return true;
        }

        private static void ReadElementResults(
            JsonElement root,
            CircuitDto dto,
            SimulationResultDto result,
            bool includeLines,
            bool includeLoads)
        {
            if (!TryGetProperty(root, out JsonElement elementos, "elementos", "elements") ||
                elementos.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (JsonProperty item in elementos.EnumerateObject())
            {
                string key = NormalizeElementName(item.Name);
                double[] correntes = ReadPolarMagnitudes(item.Value, "correntes", "currents");

                if (correntes.Length == 0)
                    continue;

                double corrente = correntes[0];

                if (includeLines && TryResolveLineId(dto, key, out string lineId))
                {
                    result.Lines.Add(new LineResultDto
                    {
                        Id = lineId,
                        Corrente = corrente,
                        CorrenteLinha = corrente,
                        CorrenteFaseA = ReadArrayValue(correntes, 0),
                        AnguloFaseA = 0,
                        CorrenteFaseB = ReadArrayValue(correntes, 1),
                        AnguloFaseB = -120,
                        CorrenteFaseC = ReadArrayValue(correntes, 2),
                        AnguloFaseC = 120
                    });
                }
                else if (includeLoads && TryResolveLoadId(dto, key, out string loadId))
                {
                    result.Loads.Add(new LoadResultDto
                    {
                        Id = loadId,
                        Corrente = corrente,
                        CorrenteLinha = corrente,
                        CorrenteFaseA = ReadArrayValue(correntes, 0),
                        AnguloFaseA = 0,
                        CorrenteFaseB = ReadArrayValue(correntes, 1),
                        AnguloFaseB = -120,
                        CorrenteFaseC = ReadArrayValue(correntes, 2),
                        AnguloFaseC = 120
                    });
                }
            }
        }

        private static string ResolveLineId(CircuitDto dto, string key)
        {
            return TryResolveLineId(dto, key, out string id) ? id : key;
        }

        private static string ResolveLoadId(CircuitDto dto, string key)
        {
            return TryResolveLoadId(dto, key, out string id) ? id : key;
        }

        private static bool TryResolveLineId(CircuitDto dto, string key, out string id)
        {
            key = NormalizeElementName(key);

            foreach (LineDto line in dto.Lines)
            {
                if (string.Equals(line.Id, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line.Nome, key, StringComparison.OrdinalIgnoreCase))
                {
                    id = line.Id;
                    return true;
                }
            }

            id = string.Empty;
            return false;
        }

        private static bool TryResolveLoadId(CircuitDto dto, string key, out string id)
        {
            key = NormalizeElementName(key);

            foreach (LoadDto load in dto.Loads)
            {
                if (string.Equals(load.Id, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(load.Nome, key, StringComparison.OrdinalIgnoreCase))
                {
                    id = load.Id;
                    return true;
                }
            }

            id = string.Empty;
            return false;
        }

        private static string NormalizeElementName(string value)
        {
            int separator = value.IndexOf('.');
            return separator >= 0 ? value[(separator + 1)..] : value;
        }

        private static string ReadString(JsonElement element, params string[] names)
        {
            foreach (string name in names)
            {
                if (TryGetProperty(element, out JsonElement value, name) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static IList<string> ReadStringArray(JsonElement element, params string[] names)
        {
            var values = new List<string>();

            if (!TryGetProperty(element, out JsonElement array, names) ||
                array.ValueKind != JsonValueKind.Array)
            {
                return values;
            }

            foreach (JsonElement item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                    values.Add(item.GetString() ?? string.Empty);
            }

            return values;
        }

        private static double ReadDouble(JsonElement element, params string[] names)
        {
            return ReadNullableDouble(element, names) ?? 0;
        }

        private static double? ReadNullableDouble(JsonElement element, params string[] names)
        {
            if (!TryGetProperty(element, out JsonElement value, names))
                return null;

            if (value.TryGetDouble(out double result))
                return result;

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            return null;
        }

        private static double[] ReadPolarMagnitudes(JsonElement element, params string[] names)
        {
            if (!TryGetProperty(element, out JsonElement array, names) ||
                array.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<double>();
            }

            return array.EnumerateArray()
                .Select(item => ReadDouble(item, "mag", "magnitude"))
                .Where(value => value > 0)
                .ToArray();
        }

        private static double? ReadArrayValue(double[] values, int index)
        {
            return index >= 0 && index < values.Length
                ? values[index]
                : null;
        }

        private static bool TryGetProperty(JsonElement element, out JsonElement value, params string[] names)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (names.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    value = property.Value;
                    return value.ValueKind != JsonValueKind.Undefined;
                }
            }

            value = default;
            return false;
        }
    }
}
