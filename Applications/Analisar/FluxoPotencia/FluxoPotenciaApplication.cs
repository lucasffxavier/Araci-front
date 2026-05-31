using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.Applications.Simulation;
using Araci.DTOs;

namespace Araci.Applications.Analisar.FluxoPotencia
{
    public class FluxoPotenciaApplication
    {
        private readonly CircuitDtoBuilder _circuitBuilder;
        private readonly ISimulationGateway _gateway;

        public FluxoPotenciaApplication(
            CircuitDtoBuilder circuitBuilder,
            ISimulationGateway gateway)
        {
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
