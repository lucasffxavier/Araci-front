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
    }
}