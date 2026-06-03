using System.Windows;
using System.Windows.Controls;

namespace Araci.Ribbon.Tabs
{
    public partial class DesenharTab : UserControl
    {
        public DesenharTab()
        {
            InitializeComponent();
        }

        private void LinhaButton_Click(object sender, RoutedEventArgs e)
        {
            FocarViewport();
        }

        private void RetanguloButton_Click(object sender, RoutedEventArgs e)
        {
            FocarViewport();
        }

        private void CirculoButton_Click(object sender, RoutedEventArgs e)
        {
            FocarViewport();
        }

        private void TextoButton_Click(object sender, RoutedEventArgs e)
        {
            FocarViewport();
        }

        private void FocarViewport()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.FocarViewport();
        }
    }
}
