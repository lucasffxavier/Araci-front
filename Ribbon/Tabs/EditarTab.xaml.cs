using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Araci;
using Araci.Applications.Editar.Alinhar;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Deletar;
using Araci.Applications.Editar.Mover;
using Araci.Applications.Editar.Selecionar;
using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class EditarTab : UserControl
    {
        private readonly Brush _brushNormal = Brushes.Transparent;
        private readonly Brush _brushAtivo = new SolidColorBrush(Color.FromRgb(210, 230, 255));
        private readonly Brush _bordaNormal = Brushes.Transparent;
        private readonly Brush _bordaAtiva = new SolidColorBrush(Color.FromRgb(80, 140, 220));
        private EditorContext? _contextAssinado;

        public EditarTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private EditorContext? Context => DataContext as EditorContext;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AssinarEventoFerramenta();
            AtualizarBotoes(Context?.Tools.FerramentaAtual);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DesassinarEventoFerramenta();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AssinarEventoFerramenta();
            AtualizarBotoes(Context?.Tools.FerramentaAtual);
        }

        private void AssinarEventoFerramenta()
        {
            if (_contextAssinado == Context)
                return;

            DesassinarEventoFerramenta();

            if (Context == null)
                return;

            Context.Tools.FerramentaAlterada += OnFerramentaAlterada;
            _contextAssinado = Context;
        }

        private void DesassinarEventoFerramenta()
        {
            if (_contextAssinado == null)
                return;

            _contextAssinado.Tools.FerramentaAlterada -= OnFerramentaAlterada;
            _contextAssinado = null;
        }

        private void OnFerramentaAlterada(ITool tool)
        {
            AtualizarBotoes(tool);
        }

        private void AtualizarBotoes(ITool? tool)
        {
            ResetarBotoes();

            if (tool == null || !tool.MantemBotaoAtivado)
                return;

            AtivarSeCorresponder(SelecionarButton, tool);
            AtivarSeCorresponder(MoverButton, tool);
            AtivarSeCorresponder(AlinharButton, tool);
            AtivarSeCorresponder(DeletarButton, tool);
        }

        private void AtivarSeCorresponder(Button button, ITool tool)
        {
            if (button.Tag is not string key || !FerramentaCorresponde(tool, key))
                return;

            button.Background = _brushAtivo;
            button.BorderBrush = _bordaAtiva;
            button.BorderThickness = new Thickness(1);
        }

        private void ResetarBotoes()
        {
            ResetarBotao(SelecionarButton);
            ResetarBotao(MoverButton);
            ResetarBotao(AlinharButton);
            ResetarBotao(DeletarButton);
        }

        private void ResetarBotao(Button button)
        {
            button.Background = _brushNormal;
            button.BorderBrush = _bordaNormal;
            button.BorderThickness = new Thickness(1);
        }

        private bool FerramentaCorresponde(ITool tool, string key)
        {
            var nome = Normalizar(tool.Nome);
            var chave = Normalizar(key);
            return nome == chave || nome.Contains(chave, StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalizar(string valor)
        {
            return valor.Replace(" ", "").Replace("_", "").Replace("-", "").Trim();
        }

        private void SelecionarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            Context.Tools.VoltarParaSelecao();
            FocarViewport();
        }

        private void MoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            Context.Tools.AtivarMover();
            FocarViewport();
        }

        private void AlinharButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            Context.Tools.AtivarAlinhar();
            FocarViewport();
        }

        private void CopiarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            Context.Clipboard.CopiarSelecionados();
            FocarViewport();
        }

        private void ColarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            Context.Clipboard.Colar();
            FocarViewport();
        }

        private void DeletarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            Context.Tools.AtivarFerramenta(new DeletarTool(Context));
            FocarViewport();
        }

        private void DesfazerButton_Click(object sender, RoutedEventArgs e)
        {
            Context?.Commands.Undo();
            FocarViewport();
        }

        private void RefazerButton_Click(object sender, RoutedEventArgs e)
        {
            Context?.Commands.Redo();
            FocarViewport();
        }

        private void FocarViewport()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.FocarViewport();
        }
    }
}
