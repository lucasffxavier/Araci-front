using System.Windows;

namespace Araci.Properties
{
    public partial class ElementosTabelaWindow : Window
    {
        public ElementosTabelaWindow()
        {
            InitializeComponent();
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
