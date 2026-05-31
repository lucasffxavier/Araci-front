using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.Applications.Simulation;
using Araci.DTOs;
using Araci.Infrastructure.Simulation;
using Araci.Services;

namespace Araci.Applications.Analisar.FluxoPotencia
{
    public class FluxoPotenciaApplication
    {
        private readonly CircuitDtoBuilder _circuitBuilder;
        private readonly ISimulationGateway _gateway;

        public FluxoPotenciaApplication(EditorContext context)
            : this(
                context,
                new CircuitDtoBuilder(context?.Document ?? throw new ArgumentNullException(nameof(context))),
                new FastApiOpenDssGateway())
        {
        }

        public FluxoPotenciaApplication(
            EditorContext context,
            CircuitDtoBuilder circuitBuilder,
            ISimulationGateway gateway)
        {
            ArgumentNullException.ThrowIfNull(context);
            _circuitBuilder = circuitBuilder ?? throw new ArgumentNullException(nameof(circuitBuilder));
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public string Resultado { get; private set; } = string.Empty;

        public async Task ExecutarAsync()
        {
            CircuitDto dto = _circuitBuilder.Build();
            Resultado = await _gateway.SimularTextoAsync(dto);

            Debug.WriteLine(Resultado);
        }
    }
}
