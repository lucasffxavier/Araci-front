using System.Windows;
using System.Windows.Controls;
using Araci.Applications.Analisar.FluxoDeCorrente;
using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class AnaliseTab : UserControl
    {
        public AnaliseTab()
        {
            InitializeComponent();
        }

        private EditorContext? Context =>
            DataContext as EditorContext;

        private async void FluxoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null) return;

            FluxoDeCorrenteApplication app = new(Context);

            await app.ExecutarAsync();
        }
    }
}
