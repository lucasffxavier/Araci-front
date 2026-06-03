using System.Windows;
using System.Windows.Controls;

namespace Araci.Ribbon
{
    public partial class RibbonView
        : UserControl
    {
        public RibbonView()
        {
            InitializeComponent();
        }

        private void Arquivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                Window.GetWindow(this)
                as MainWindow;

            window?.ToggleArquivoMenu(
                ArquivoButton);
        }

        private void MainTabs_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Evita processar quando o evento vem de controles filhos
            if (e.OriginalSource != MainTabs)
                return;

            // Guard: InitializeComponent ainda não terminou
            if (TabDiagrama == null)
                return;

            TabDiagrama.Visibility = Visibility.Collapsed;
            TabAnotar.Visibility = Visibility.Collapsed;
            TabEditar.Visibility = Visibility.Collapsed;
            TabAnalise.Visibility = Visibility.Collapsed;
            TabGerenciar.Visibility = Visibility.Collapsed;

            switch (MainTabs.SelectedIndex)
            {
                case 0: TabDiagrama.Visibility = Visibility.Visible; break;
                case 1: TabAnotar.Visibility = Visibility.Visible; break;
                case 2: TabEditar.Visibility = Visibility.Visible; break;
                case 3: TabAnalise.Visibility = Visibility.Visible; break;
                case 4: TabGerenciar.Visibility = Visibility.Visible; break;
            }
        }
    }
}
