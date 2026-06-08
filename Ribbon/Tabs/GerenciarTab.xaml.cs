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

        private void NavegadorButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.AlternarNavegadorProjeto();
        }

        private void NovaVistaButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.CriarNovaVistaProjeto();
        }

        private void NovaTabelaButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.CriarNovaTabelaProjeto();
        }

        private void NovaPranchaButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.CriarNovaPranchaProjeto();
        }

        private void UnidadesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.MostrarConfiguracaoUnidades();
        }
    }
}
