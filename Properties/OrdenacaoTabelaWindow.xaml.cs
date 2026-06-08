using System.Windows;

namespace Araci.Properties
{
    public partial class OrdenacaoTabelaWindow : Window
    {
        public OrdenacaoTabelaWindow()
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
