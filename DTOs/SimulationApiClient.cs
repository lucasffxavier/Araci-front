using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Araci.DTOs
{
    public class SimulationApiClient
    {
        private const string SimulationUrl = "http://localhost:5000/simular";

        private readonly HttpClient _httpClient;

        public SimulationApiClient()
            : this(new HttpClient())
        {
        }

        public SimulationApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> SimularAsync(CircuitDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                string json = JsonSerializer.Serialize(dto);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using HttpResponseMessage response = await _httpClient.PostAsync(
                    SimulationUrl,
                    content);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Erro ao simular circuito. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Resposta: {responseBody}");
                }

                return responseBody;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException(
                    "Tempo limite excedido ao chamar a API de simulacao.",
                    ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Erro ao enviar circuito para a API de simulacao.",
                    ex);
            }
        }
    }
}
