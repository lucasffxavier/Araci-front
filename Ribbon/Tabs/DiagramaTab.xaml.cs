using System;
using System.Windows;
using System.Windows.Controls;

using Araci.Applications.Diagrama.InserirCabo;
using Araci.Applications.Diagrama.InserirCarga;
using Araci.Applications.Diagrama.InserirGerador;
using Araci.Applications.Diagrama.InserirBarra;
using Araci.Applications.Diagrama.InserirSin;
using Araci.Applications.Diagrama.InserirTransformador;

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
        // CONTEXT
        // =========================

        private EditorContext? Context =>
            DataContext as EditorContext;

        // =========================
        // GERADOR
        // =========================

        private void GeradorButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Context == null) return;

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
            if (Context == null) return;

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
            if (Context == null) return;

            InserirCaboApplication app =
                new(Context);

            app.Executar();
        }

        // =========================
        // SIN
        // =========================

        private void SinButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Context == null) return;

            InserirSinApplication app =
                new(Context);

            app.Executar();
        }

        // =========================
        // TRANSFORMADOR
        // =========================

        private void TransformadorButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Context == null) return;

            InserirTransformadorApplication app =
                new(Context);

            app.Executar();
        }

        // =========================
        // BARRA
        // =========================

        private void BarraButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Context == null) return;

            InserirBarraApplication app =
                new(Context);

            app.Executar();
        }

        // =========================
        // LOADED
        // =========================

        private void UserControl_Loaded(
            object sender,
            RoutedEventArgs e)
        {
        }
    }
}
