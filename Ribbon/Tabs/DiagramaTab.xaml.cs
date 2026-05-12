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

        // =========================
        // GERADOR
        // =========================

        private void GeradorButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (AppServices.Viewport == null)
                return;

            InserirGeradorApplication app =
                new(AppServices.Viewport);

            app.Executar();
        }

        // =========================
        // CARGA
        // =========================

        private void CargaButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (AppServices.Viewport == null)
                return;

            InserirCargaApplication app =
                new(AppServices.Viewport);

            app.Executar();
        }

        // =========================
        // CABO
        // =========================

        private void CaboButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (AppServices.Viewport == null)
                return;

            InserirCaboApplication app =
                new(AppServices.Viewport);

            app.Executar();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}