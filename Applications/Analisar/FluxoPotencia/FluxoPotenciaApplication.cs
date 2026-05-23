using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Araci.DTOs;
using Araci.Services;

namespace Araci.Applications.Analisar.FluxoPotencia
{
    public class FluxoPotenciaApplication
    {
        private readonly EditorContext _context;

        public FluxoPotenciaApplication(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Resultado { get; private set; } = string.Empty;

        public async Task ExecutarAsync()
        {
            ParameterReader reader = new(_context);
            CircuitBuilder builder = new(reader);
            CircuitDto dto = builder.Build();

            SimulationApiClient client = new();

            Resultado = await client.SimularAsync(dto);

            Debug.WriteLine(Resultado);
        }
    }
}
