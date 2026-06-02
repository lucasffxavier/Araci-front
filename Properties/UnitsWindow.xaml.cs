using System.Windows;
using Araci.ViewModels;

namespace Araci.Properties
{
    public partial class UnitsWindow : Window
    {
        public UnitsWindow(UnitsSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
