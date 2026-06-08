using Araci.Services.Simulation;

namespace Araci.Applications.Abstractions
{
    public interface IUserDialogService
    {
        void ShowInfo(string title, string message);

        void ShowWarning(string title, string message);

        void ShowError(string title, string message);

        void ShowElementosTabelaPlaceholder();

        bool Confirm(string title, string message);

        void Show(SimulationMessage message);
    }
}
