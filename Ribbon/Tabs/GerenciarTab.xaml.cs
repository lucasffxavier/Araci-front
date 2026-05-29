using System.Windows;
using System.Windows.Controls;

namespace Araci.Ribbon.Tabs
{
    public partial class GerenciarTab : UserControl
    {
        public GerenciarTab()
        {
            InitializeComponent();
        }

        private void PropriedadesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.MostrarPropriedades();
        }
    }
}