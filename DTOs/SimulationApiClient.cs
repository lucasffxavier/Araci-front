using System;
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

        private static readonly JsonSerializerOptions RequestJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions ResponseJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly string _simulationUrl;

        public SimulationApiClient()
            : this(new HttpClient(), DefaultSimulationUrl)
        {
        }

        public SimulationApiClient(string simulationUrl)
            : this(new HttpClient(), simulationUrl)
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

        public async Task<string> SimularAsync(CircuitDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                string json = JsonSerializer.Serialize(dto, RequestJsonOptions);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using HttpResponseMessage response = await _httpClient.PostAsync(
                    _simulationUrl,
                    content);

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
            SimulationResultDto? direct = JsonSerializer.Deserialize<SimulationResultDto>(
                responseBody,
                ResponseJsonOptions);

            if (direct != null && (direct.Lines.Count > 0 || direct.Loads.Count > 0))
                return direct;

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("resultado", out JsonElement resultado))
                root = resultado;

            var result = new SimulationResultDto();

            if (root.TryGetProperty("linhas", out JsonElement linhas) && linhas.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement line in linhas.EnumerateArray())
                {
                    string key = ReadString(line, "id", "linha", "nome");
                    double corrente = ReadDouble(line, "corrente");

                    if (corrente == 0 && line.TryGetProperty("correntes", out JsonElement correntes))
                        corrente = ReadFirstNumber(correntes);

                    result.Lines.Add(new LineResultDto
                    {
                        Id = ResolveLineId(dto, key),
                        Corrente = corrente
                    });
                }
            }

            if (root.TryGetProperty("cargas", out JsonElement cargas) && cargas.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement load in cargas.EnumerateArray())
                {
                    string key = ReadString(load, "id", "carga", "nome");

                    result.Loads.Add(new LoadResultDto
                    {
                        Id = ResolveLoadId(dto, key),
                        Corrente = ReadDouble(load, "corrente")
                    });
                }
            }

            return result;
        }

        private static string ResolveLineId(CircuitDto dto, string key)
        {
            foreach (LineDto line in dto.Lines)
            {
                if (string.Equals(line.Id, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line.Nome, key, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Id;
                }
            }

            return key;
        }

        private static string ResolveLoadId(CircuitDto dto, string key)
        {
            foreach (LoadDto load in dto.Loads)
            {
                if (string.Equals(load.Id, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(load.Nome, key, StringComparison.OrdinalIgnoreCase))
                {
                    return load.Id;
                }
            }

            return key;
        }

        private static string ReadString(JsonElement element, params string[] names)
        {
            foreach (string name in names)
            {
                if (element.TryGetProperty(name, out JsonElement value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static double ReadDouble(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out JsonElement value) && value.TryGetDouble(out double result)
                ? result
                : 0;
        }

        private static double ReadFirstNumber(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array)
                return 0;

            foreach (JsonElement item in element.EnumerateArray())
            {
                if (item.TryGetDouble(out double value))
                    return value;
            }

            return 0;
        }
    }
}
