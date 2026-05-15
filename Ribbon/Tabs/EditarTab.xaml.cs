using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Deletar;
using Araci.Applications.Editar.Mover;
using Araci.Applications.Editar.Selecionar;

using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class EditarTab : UserControl
    {
        // =========================
        // CORES
        // =========================

        private readonly Brush _brushNormal = Brushes.Transparent;

        private readonly Brush _brushAtivo =  new SolidColorBrush(Color.FromRgb(210, 230, 255));

        private bool _eventoFerramentaAssinado;

        // =========================
        // CONSTRUTOR
        // =========================

        public EditarTab()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            Unloaded += OnUnloaded;
        }

        // =========================
        // LOADED
        // =========================

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AssinarEventoFerramenta();

            AtualizarBotoes(AppServices.Tools.FerramentaAtual);
        }

        // =========================
        // UNLOADED
        // =========================

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DesassinarEventoFerramenta();
        }

        // =========================
        // EVENTOS
        // =========================

        private void AssinarEventoFerramenta()
        {
            if (_eventoFerramentaAssinado)
                return;

            AppServices.Tools.FerramentaAlterada +=
                OnFerramentaAlterada;

            _eventoFerramentaAssinado =
                true;
        }

        private void DesassinarEventoFerramenta()
        {
            if (!_eventoFerramentaAssinado)
                return;

            AppServices.Tools.FerramentaAlterada -=
                OnFerramentaAlterada;

            _eventoFerramentaAssinado =
                false;
        }

        // =========================
        // EVENTO TOOL
        // =========================

        private void OnFerramentaAlterada(ITool tool)
        {
            AtualizarBotoes(tool);
        }

        // =========================
        // VISUAL
        // =========================

        private void AtualizarBotoes(ITool tool)
        {
            ResetarBotoes();

            if (!tool.MantemBotaoAtivado)
                return;

            switch (tool.Nome)
            {
                case "Selecionar":

                    SelecionarButton.Background = _brushAtivo;

                    break;

                case "Mover":

                    MoverButton.Background = _brushAtivo;

                    break;

                case "Deletar":

                    DeletarButton.Background = _brushAtivo;

                    break;
            }
        }

        private void ResetarBotoes()
        {
            SelecionarButton.Background =
                _brushNormal;

            MoverButton.Background =
                _brushNormal;

            DeletarButton.Background =
                _brushNormal;
        }

        // =========================
        // BOTÕES
        // =========================

        private void SelecionarButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Tools.AtivarFerramenta(
                new SelecionarTool(AppServices.Current));
        }

        private void MoverButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Tools.AtivarFerramenta(
                new MoverTool(AppServices.Current));
        }

        private void CopiarButton_Click(object sender, RoutedEventArgs e)
        {
            ClipboardService.CopiarSelecionados(
                AppServices.Current);
        }

        private void ColarButton_Click(object sender, RoutedEventArgs e)
        {
            ClipboardService.Colar(
                AppServices.Current);
        }

        private void DeletarButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Tools.AtivarFerramenta(
                new DeletarTool(AppServices.Current));
        }

        // =========================
        // UNDO
        // =========================

        private void DesfazerButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Commands.Undo();
        }

        // =========================
        // REDO
        // =========================

        private void RefazerButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.Commands.Redo();
        }
    }
}
