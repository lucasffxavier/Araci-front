using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Araci.Core.Documents;

namespace Araci.Properties
{
    public partial class ExibicaoTabelaWindow : Window
    {
        private readonly ProjectTableDisplaySettings _original;

        private static readonly string[] FontesPrincipais =
        {
            "Arial",
            "Calibri",
            "Segoe UI",
            "Times New Roman",
            "Verdana",
            "Tahoma",
            "Consolas",
            "Courier New",
            "Georgia",
            "Cambria",
            "Trebuchet MS",
            "Lucida Console"
        };

        public ExibicaoTabelaWindow(ProjectTableDisplaySettings exibicao)
        {
            InitializeComponent();
            _original = exibicao?.CriarCopia() ?? new ProjectTableDisplaySettings();
            ConfigurarCombos();
            AplicarValores(_original);
            AtualizarTodosBotoesCor();
        }

        public ProjectTableDisplaySettings Exibicao { get; private set; } = new();

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Exibicao = LerValores();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AtualizarTodosBotoesCor();
        }

        private void CorTextoTituloButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorTextoTituloTextBox);
        }

        private void CorFundoTituloButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorFundoTituloTextBox);
        }

        private void CorTextoCabecalhoButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorTextoCabecalhoTextBox);
        }

        private void CorFundoCabecalhoButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorFundoCabecalhoTextBox);
        }

        private void CorTextoCorpoButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorTextoCorpoTextBox);
        }

        private void CorFundoCorpoButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorFundoCorpoTextBox);
        }

        private void CorLinhaAlternadaButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorLinhaAlternadaTextBox);
        }

        private void CorGradeButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorGradeTextBox);
        }

        private void CorContornoButton_Click(object sender, RoutedEventArgs e)
        {
            AbrirSeletorCor(CorContornoTextBox);
        }

        private void ConfigurarCombos()
        {
            FonteTituloComboBox.ItemsSource = FontesPrincipais;
            FonteCabecalhoComboBox.ItemsSource = FontesPrincipais;
            FonteCorpoComboBox.ItemsSource = FontesPrincipais;

            var alinhamentos = Enum.GetValues(typeof(ProjectTableTextAlignment))
                .Cast<ProjectTableTextAlignment>()
                .ToList();

            AlinhamentoTituloComboBox.ItemsSource = alinhamentos;
            AlinhamentoCabecalhoComboBox.ItemsSource = alinhamentos;
            AlinhamentoCorpoComboBox.ItemsSource = alinhamentos;
        }

        private void AplicarValores(ProjectTableDisplaySettings valor)
        {
            ExibirTituloCheckBox.IsChecked = valor.ExibirTitulo;
            AplicarFonte(FonteTituloComboBox, valor.FonteTitulo);
            TamanhoTituloTextBox.Text = Format(valor.TamanhoFonteTitulo);
            TituloNegritoCheckBox.IsChecked = valor.TituloNegrito;
            CorTextoTituloTextBox.Text = valor.CorTextoTitulo;
            CorFundoTituloTextBox.Text = valor.CorFundoTitulo;
            AlturaTituloTextBox.Text = Format(valor.AlturaTitulo);
            AlinhamentoTituloComboBox.SelectedItem = valor.AlinhamentoTitulo;

            ExibirCabecalhoCheckBox.IsChecked = valor.ExibirCabecalho;
            AplicarFonte(FonteCabecalhoComboBox, valor.FonteCabecalho);
            TamanhoCabecalhoTextBox.Text = Format(valor.TamanhoFonteCabecalho);
            CabecalhoNegritoCheckBox.IsChecked = valor.CabecalhoNegrito;
            CorTextoCabecalhoTextBox.Text = valor.CorTextoCabecalho;
            CorFundoCabecalhoTextBox.Text = valor.CorFundoCabecalho;
            AlturaCabecalhoTextBox.Text = Format(valor.AlturaCabecalho);
            AlinhamentoCabecalhoComboBox.SelectedItem = valor.AlinhamentoCabecalho;

            AplicarFonte(FonteCorpoComboBox, valor.FonteCorpo);
            TamanhoCorpoTextBox.Text = Format(valor.TamanhoFonteCorpo);
            CorTextoCorpoTextBox.Text = valor.CorTextoCorpo;
            CorFundoCorpoTextBox.Text = valor.CorFundoCorpo;
            AlturaLinhaCorpoTextBox.Text = Format(valor.AlturaLinhaCorpo);
            AlinhamentoCorpoComboBox.SelectedItem = valor.AlinhamentoCorpo;
            LinhasAlternadasCheckBox.IsChecked = valor.UsarLinhasAlternadas;
            CorLinhaAlternadaTextBox.Text = valor.CorLinhaAlternada;

            ExibirGradeCheckBox.IsChecked = valor.ExibirLinhasGrade;
            CorGradeTextBox.Text = valor.CorGrade;
            EspessuraGradeTextBox.Text = Format(valor.EspessuraGrade);
            ExibirContornoCheckBox.IsChecked = valor.ExibirContornoExterno;
            CorContornoTextBox.Text = valor.CorContorno;
            EspessuraContornoTextBox.Text = Format(valor.EspessuraContorno);
        }

        private ProjectTableDisplaySettings LerValores()
        {
            return new ProjectTableDisplaySettings
            {
                ExibirTitulo = ExibirTituloCheckBox.IsChecked == true,
                FonteTitulo = NormalizarTexto(FonteTituloComboBox.Text, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteTitulo = LerDouble(TamanhoTituloTextBox.Text, _original.TamanhoFonteTitulo),
                TituloNegrito = TituloNegritoCheckBox.IsChecked == true,
                CorTextoTitulo = NormalizarCor(CorTextoTituloTextBox.Text, ProjectTableDisplaySettings.DefaultTitleTextColor),
                CorFundoTitulo = NormalizarCor(CorFundoTituloTextBox.Text, ProjectTableDisplaySettings.DefaultTitleBackgroundColor),
                AlturaTitulo = LerDouble(AlturaTituloTextBox.Text, _original.AlturaTitulo),
                AlinhamentoTitulo = AlinhamentoTituloComboBox.SelectedItem is ProjectTableTextAlignment alinhamentoTitulo ? alinhamentoTitulo : _original.AlinhamentoTitulo,
                ExibirCabecalho = ExibirCabecalhoCheckBox.IsChecked == true,
                FonteCabecalho = NormalizarTexto(FonteCabecalhoComboBox.Text, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteCabecalho = LerDouble(TamanhoCabecalhoTextBox.Text, _original.TamanhoFonteCabecalho),
                CabecalhoNegrito = CabecalhoNegritoCheckBox.IsChecked == true,
                CorTextoCabecalho = NormalizarCor(CorTextoCabecalhoTextBox.Text, ProjectTableDisplaySettings.DefaultHeaderTextColor),
                CorFundoCabecalho = NormalizarCor(CorFundoCabecalhoTextBox.Text, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor),
                AlturaCabecalho = LerDouble(AlturaCabecalhoTextBox.Text, _original.AlturaCabecalho),
                AlinhamentoCabecalho = AlinhamentoCabecalhoComboBox.SelectedItem is ProjectTableTextAlignment alinhamentoCabecalho ? alinhamentoCabecalho : ProjectTableTextAlignment.Esquerda,
                FonteCorpo = NormalizarTexto(FonteCorpoComboBox.Text, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteCorpo = LerDouble(TamanhoCorpoTextBox.Text, _original.TamanhoFonteCorpo),
                CorTextoCorpo = NormalizarCor(CorTextoCorpoTextBox.Text, ProjectTableDisplaySettings.DefaultBodyTextColor),
                CorFundoCorpo = NormalizarCor(CorFundoCorpoTextBox.Text, ProjectTableDisplaySettings.DefaultBodyBackgroundColor),
                AlturaLinhaCorpo = LerDouble(AlturaLinhaCorpoTextBox.Text, _original.AlturaLinhaCorpo),
                AlinhamentoCorpo = AlinhamentoCorpoComboBox.SelectedItem is ProjectTableTextAlignment alinhamentoCorpo ? alinhamentoCorpo : ProjectTableTextAlignment.Esquerda,
                UsarLinhasAlternadas = LinhasAlternadasCheckBox.IsChecked == true,
                CorLinhaAlternada = NormalizarCor(CorLinhaAlternadaTextBox.Text, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor),
                ExibirLinhasGrade = ExibirGradeCheckBox.IsChecked == true,
                CorGrade = NormalizarCor(CorGradeTextBox.Text, ProjectTableDisplaySettings.DefaultGridColor),
                EspessuraGrade = LerDouble(EspessuraGradeTextBox.Text, _original.EspessuraGrade),
                ExibirContornoExterno = ExibirContornoCheckBox.IsChecked == true,
                CorContorno = NormalizarCor(CorContornoTextBox.Text, ProjectTableDisplaySettings.DefaultOutlineColor),
                EspessuraContorno = LerDouble(EspessuraContornoTextBox.Text, _original.EspessuraContorno)
            };
        }

        private void AbrirSeletorCor(TextBox textBox)
        {
            var window = new ColorPickerWindow(textBox.Text)
            {
                Owner = this
            };

            if (window.ShowDialog() != true)
                return;

            textBox.Text = window.SelectedColorHex;
            AtualizarTodosBotoesCor();
        }

        private void AtualizarTodosBotoesCor()
        {
            AtualizarBotaoCor(CorTextoTituloTextBox, CorTextoTituloButton, ProjectTableDisplaySettings.DefaultTitleTextColor);
            AtualizarBotaoCor(CorFundoTituloTextBox, CorFundoTituloButton, ProjectTableDisplaySettings.DefaultTitleBackgroundColor);
            AtualizarBotaoCor(CorTextoCabecalhoTextBox, CorTextoCabecalhoButton, ProjectTableDisplaySettings.DefaultHeaderTextColor);
            AtualizarBotaoCor(CorFundoCabecalhoTextBox, CorFundoCabecalhoButton, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor);
            AtualizarBotaoCor(CorTextoCorpoTextBox, CorTextoCorpoButton, ProjectTableDisplaySettings.DefaultBodyTextColor);
            AtualizarBotaoCor(CorFundoCorpoTextBox, CorFundoCorpoButton, ProjectTableDisplaySettings.DefaultBodyBackgroundColor);
            AtualizarBotaoCor(CorLinhaAlternadaTextBox, CorLinhaAlternadaButton, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor);
            AtualizarBotaoCor(CorGradeTextBox, CorGradeButton, ProjectTableDisplaySettings.DefaultGridColor);
            AtualizarBotaoCor(CorContornoTextBox, CorContornoButton, ProjectTableDisplaySettings.DefaultOutlineColor);
        }

        private static void AtualizarBotaoCor(TextBox textBox, Button button, string fallback)
        {
            string color = ColorPickerWindow.TryNormalizeColor(textBox.Text, out string normalized)
                ? normalized
                : fallback;

            button.Background = CriarBrush(color);
            button.BorderBrush = Brushes.DimGray;
            button.Content = string.Empty;
            button.ToolTip = color;
        }

        private static SolidColorBrush CriarBrush(string color)
        {
            if (!ColorPickerWindow.TryNormalizeColor(color, out string normalized))
                normalized = "#FF000000";

            byte a = byte.Parse(normalized.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte r = byte.Parse(normalized.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte g = byte.Parse(normalized.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte b = byte.Parse(normalized.Substring(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        private static void AplicarFonte(ComboBox comboBox, string? valor)
        {
            string fonte = NormalizarTexto(valor, ProjectTableDisplaySettings.DefaultFontFamily);
            comboBox.Text = fonte;
            comboBox.SelectedItem = FontesPrincipais.Contains(fonte) ? fonte : null;
        }

        private static double LerDouble(string? texto, double fallback)
        {
            if (double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out double valor))
                return valor;

            if (double.TryParse(texto, NumberStyles.Float, CultureInfo.GetCultureInfo("pt-BR"), out valor))
                return valor;

            return fallback;
        }

        private static string NormalizarCor(string? valor, string fallback)
        {
            return ColorPickerWindow.TryNormalizeColor(valor, out string normalized)
                ? normalized
                : fallback;
        }

        private static string NormalizarTexto(string? valor, string fallback)
        {
            return string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
        }

        private static string Format(double valor)
        {
            return valor.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}