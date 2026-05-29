using System.Windows;
using System.Windows.Controls;
using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class DiagramaTab : UserControl
    {
        public DiagramaTab()
        {
            InitializeComponent();
        }

        private EditorContext? Context => DataContext as EditorContext;

        private void ElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            if (sender is not Button button || button.Tag is not string kind)
                return;

            Context.Tools.AtivarInsercaoElemento(kind);
            FocarViewport();
        }

        private void FocarViewport()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.FocarViewport();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}