using System;
using System.Linq;
using System.Threading.Tasks;
using Araci.API;
using Araci.DTOs;
using Araci.Models;
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

        public async Task ExecutarAsync()
        {
            CoreApi api = new(_context);
            ParameterReader reader = new(api);
            CircuitBuilder builder = new(reader);
            CircuitDto dto = builder.Build();
            SimulationApiClient client = new();

            Resultado = await client.SimularTipadoAsync(dto);

            AplicarResultado(api, Resultado);
        }

        private static void AplicarResultado(CoreApi api, SimulationResultDto resultado)
        {
            foreach (LineResultDto lineResult in resultado.Lines)
            {
                Cabo? cabo = api.ObterElementos<Cabo>()
                    .FirstOrDefault(c => string.Equals(c.Id.ToString(), lineResult.Id, StringComparison.OrdinalIgnoreCase));

                if (cabo != null)
                    cabo.CorrenteLinha = FormatCurrent(lineResult.Corrente);
            }

            foreach (LoadResultDto loadResult in resultado.Loads)
            {
                Carga? carga = api.ObterElementos<Carga>()
                    .FirstOrDefault(c => string.Equals(c.Id.ToString(), loadResult.Id, StringComparison.OrdinalIgnoreCase));

                if (carga != null)
                    carga.CorrenteLinha = FormatCurrent(loadResult.Corrente);
            }
        }

        private static string FormatCurrent(double corrente)
        {
            return $"{corrente:0.###} A";
        }
    }
}
