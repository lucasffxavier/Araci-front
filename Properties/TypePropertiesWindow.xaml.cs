using System.Windows;

namespace Araci.Properties
{
    public partial class TypePropertiesWindow : Window
    {
        public TypePropertiesWindow()
        {
            InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelarClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
