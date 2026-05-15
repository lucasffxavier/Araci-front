using System.Windows;
using System.Windows.Controls;

using Araci.Applications.Diagrama.InserirCabo;
using Araci.Applications.Diagrama.InserirCarga;
using Araci.Applications.Diagrama.InserirGerador;

using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class DiagramaTab : UserControl
    {
        // =========================
        // CONSTRUTOR
        // =========================

        public DiagramaTab()
        {
            InitializeComponent();
        }

        private EditorContext Context =>
            AppServices.Current;

        // =========================
        // GERADOR
        // =========================

        private void GeradorButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            InserirGeradorApplication app =
                new(Context);

            app.Executar();
        }

        // =========================
        // CARGA
        // =========================

        private void CargaButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            InserirCargaApplication app =
                new(Context);

            app.Executar();
        }

        // =========================
        // CABO
        // =========================

        private void CaboButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            InserirCaboApplication app =
                new(Context);

            app.Executar();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
