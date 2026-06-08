using Araci.Applications.Abstractions;
using Araci.Properties;
using Araci.Services.Simulation;
using System.Windows;

namespace Araci.Services.UI
{
    public class DialogService : IUserDialogService
    {
        public void ShowInfo(string title, string message)
        {
            Show(title, message, MessageBoxImage.Information);
        }

        public void ShowWarning(string title, string message)
        {
            Show(title, message, MessageBoxImage.Warning);
        }

        public void ShowError(string title, string message)
        {
            Show(title, message, MessageBoxImage.Error);
        }

        public void ShowElementosTabelaPlaceholder()
        {
            var window = new ElementosTabelaWindow
            {
                Owner = Application.Current?.MainWindow
            };

            window.ShowDialog();
        }

        public bool Confirm(string title, string message)
        {
            MessageBoxResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        public void Show(SimulationMessage message)
        {
            if (message == null)
                return;

            MessageBox.Show(message.Text, message.Title, MessageBoxButton.OK, message.Icon);
        }

        private static void Show(string title, string message, MessageBoxImage icon)
        {
            MessageBox.Show(
                message ?? string.Empty,
                string.IsNullOrWhiteSpace(title) ? "Araci" : title,
                MessageBoxButton.OK,
                icon);
        }
    }
}
