using System;
using System.Threading.Tasks;
using Araci.Applications.Abstractions;
using Araci.DTOs;

namespace Araci.Infrastructure.Simulation
{
    public sealed class FastApiOpenDssGateway : ISimulationGateway
    {
        private readonly SimulationApiClient _client;

        public FastApiOpenDssGateway()
            : this(new SimulationApiClient())
        {
        }

        public FastApiOpenDssGateway(SimulationApiClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<SimulationResultDto> SimularAsync(CircuitDto circuit)
        {
            return _client.SimularTipadoAsync(circuit);
        }

        public Task<string> SimularTextoAsync(CircuitDto circuit)
        {
            return _client.SimularAsync(circuit);
        }
    }
}
