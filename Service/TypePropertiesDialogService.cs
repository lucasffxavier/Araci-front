using System.Windows;
using Araci.Properties;
using Araci.ViewModels;

namespace Araci.Services
{
    public class TypePropertiesDialogService
    {
        public void Show(TipoElementoViewModel? viewModel)
        {
            if (viewModel == null)
                return;

            var window = new TypePropertiesWindow
            {
                DataContext = viewModel
            };

            if (Application.Current?.MainWindow != null)
                window.Owner = Application.Current.MainWindow;

            window.ShowDialog();
        }
    }
}
