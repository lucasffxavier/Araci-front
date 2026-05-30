using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Araci.Applications.Editar.Base;
using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class DiagramaTab : UserControl
    {
        private readonly Brush _brushNormal = Brushes.Transparent;
        private readonly Brush _brushAtivo = new SolidColorBrush(Color.FromRgb(210, 230, 255));
        private readonly Brush _bordaNormal = Brushes.Transparent;
        private readonly Brush _bordaAtiva = new SolidColorBrush(Color.FromRgb(80, 140, 220));
        private EditorContext? _contextAssinado;

        public DiagramaTab()
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
            AtualizarBotoesAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DesassinarEventoFerramenta();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AssinarEventoFerramenta();
            AtualizarBotoesAsync();
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

        private void ElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context == null)
                return;

            if (sender is not Button button || button.Tag is not string kind)
                return;

            Context.Tools.AtivarInsercaoElemento(kind);
            FocarViewport();
        }

        private void AtualizarBotoesAsync()
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (Context != null)
                    AtualizarBotoes(Context.Tools.FerramentaAtual);
                else
                    ResetarBotoes();
            }, DispatcherPriority.Loaded);
        }

        private void AtualizarBotoes(ITool tool)
        {
            ResetarBotoes();

            if (!tool.MantemBotaoAtivado)
                return;

            foreach (var button in EncontrarFilhos<Button>(ElementButtonsControl))
            {
                if (button.Tag is not string key)
                    continue;

                if (!FerramentaCorresponde(tool, key))
                    continue;

                button.Background = _brushAtivo;
                button.BorderBrush = _bordaAtiva;
                button.BorderThickness = new Thickness(1);
            }
        }

        private void ResetarBotoes()
        {
            foreach (var button in EncontrarFilhos<Button>(ElementButtonsControl))
            {
                button.Background = _brushNormal;
                button.BorderBrush = _bordaNormal;
                button.BorderThickness = new Thickness(1);
            }
        }

        private bool FerramentaCorresponde(ITool tool, string key)
        {
            var nome = Normalizar(tool.Nome);
            var chave = Normalizar(key);
            return nome == chave || nome.Contains(chave, StringComparison.OrdinalIgnoreCase) || nome.Contains("inserir" + chave, StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalizar(string valor)
        {
            return valor.Replace(" ", "").Replace("_", "").Replace("-", "").Trim();
        }

        private static IEnumerable<T> EncontrarFilhos<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            var count = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typed)
                    yield return typed;

                foreach (var descendant in EncontrarFilhos<T>(child))
                    yield return descendant;
            }
        }

        private void FocarViewport()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.FocarViewport();
        }
    }
}