using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Services;

namespace Araci.Ribbon.Tabs
{
    public partial class AnotarTab : UserControl
    {
        private readonly Brush _brushNormal = Brushes.Transparent;
        private readonly Brush _brushAtivo = new SolidColorBrush(Color.FromRgb(210, 230, 255));
        private readonly Brush _bordaNormal = Brushes.Transparent;
        private readonly Brush _bordaAtiva = new SolidColorBrush(Color.FromRgb(80, 140, 220));
        private EditorContext? _contextAssinado;

        public AnotarTab()
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
            ResetarBotao(LinhaButton);
            ResetarBotao(RetanguloButton);
            ResetarBotao(CirculoButton);
            ResetarBotao(TextoButton);

            if (tool == null || !tool.MantemBotaoAtivado)
                return;

            if (FerramentaCorresponde(tool, "Linha"))
                AtivarBotao(LinhaButton);

            if (FerramentaCorresponde(tool, "Retângulo") || FerramentaCorresponde(tool, "Retangulo"))
                AtivarBotao(RetanguloButton);

            if (FerramentaCorresponde(tool, "Círculo") || FerramentaCorresponde(tool, "Circulo"))
                AtivarBotao(CirculoButton);

            if (FerramentaCorresponde(tool, "Texto"))
                AtivarBotao(TextoButton);
        }

        private void AtivarBotao(Button button)
        {
            button.Background = _brushAtivo;
            button.BorderBrush = _bordaAtiva;
            button.BorderThickness = new Thickness(1);
        }

        private void ResetarBotao(Button button)
        {
            button.Background = _brushNormal;
            button.BorderBrush = _bordaNormal;
            button.BorderThickness = new Thickness(1);
        }

        private static bool FerramentaCorresponde(ITool tool, string key)
        {
            string nome = Normalizar(tool.Nome);
            string chave = Normalizar(key);
            return nome == chave || nome.Contains(chave, System.StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalizar(string valor)
        {
            return valor
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace("â", "a")
                .Replace("Â", "A")
                .Replace("ã", "a")
                .Replace("Ã", "A")
                .Replace("é", "e")
                .Replace("É", "E")
                .Replace("í", "i")
                .Replace("Í", "I")
                .Trim();
        }

        private void LinhaButton_Click(object sender, RoutedEventArgs e)
        {
            FocarSuperficieAtiva();
            Context?.Tools.AtivarInserirLinhaAnotativa();
        }

        private void RetanguloButton_Click(object sender, RoutedEventArgs e)
        {
            FocarSuperficieAtiva();
            Context?.Tools.AtivarInserirRetanguloAnotativo();
        }

        private void CirculoButton_Click(object sender, RoutedEventArgs e)
        {
            FocarSuperficieAtiva();
            Context?.Tools.AtivarInserirCirculoAnotativo();
        }

        private void TextoButton_Click(object sender, RoutedEventArgs e)
        {
            FocarSuperficieAtiva();
            Context?.Tools.AtivarInserirTextoAnotativo();
        }

        private void FocarViewport()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.FocarViewport();
        }

        private void FocarSuperficieAtiva()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.FocarSuperficieAtiva();
        }
    }
}