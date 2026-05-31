using System.Threading.Tasks;
using Araci.DTOs;

namespace Araci.Applications.Abstractions
{
    public interface ISimulationPipeline
    {
        Task<SimulationResultDto> ExecutarFluxoDeCorrenteAsync();
    }
}
