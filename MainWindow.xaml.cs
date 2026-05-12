using System.Windows;

namespace Araci
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ToggleArquivoMenu(UIElement target)
        {
            ArquivoPopup.PlacementTarget = target;
            ArquivoPopup.IsOpen = !ArquivoPopup.IsOpen;
        }
    }
}