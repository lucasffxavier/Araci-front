using Araci.Services;
using Araci.Services.Simulation;

namespace Araci.Applications.Abstractions
{
    public interface IUserDialogService
    {
        void ShowInfo(string title, string message);

        void ShowWarning(string title, string message);

        void ShowError(string title, string message);

        bool Confirm(string title, string message);

        void Show(SimulationMessage message);
    }
}
