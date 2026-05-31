using System.Threading.Tasks;
using Araci.DTOs;

namespace Araci.Applications.Abstractions
{
    public interface ISimulationGateway
    {
        Task<SimulationResultDto> SimularAsync(CircuitDto circuit);

        Task<string> SimularTextoAsync(CircuitDto circuit);
    }
}
