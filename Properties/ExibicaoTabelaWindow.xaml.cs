using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private void ConfigurarCombos()
        {
            FonteTituloComboBox.ItemsSource = FontesPrincipais;
            FonteCabecalhoComboBox.ItemsSource = FontesPrincipais;
            FonteCorpoComboBox.ItemsSource = FontesPrincipais;

            var alinhamentos = Enum.GetValues(typeof(ProjectTableTextAlignment))
                .Cast<ProjectTableTextAlignment>()
                .ToList();

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
                CorTextoTitulo = NormalizarTexto(CorTextoTituloTextBox.Text, ProjectTableDisplaySettings.DefaultTitleTextColor),
                CorFundoTitulo = NormalizarTexto(CorFundoTituloTextBox.Text, ProjectTableDisplaySettings.DefaultTitleBackgroundColor),
                AlturaTitulo = LerDouble(AlturaTituloTextBox.Text, _original.AlturaTitulo),
                AlinhamentoTitulo = ProjectTableTextAlignment.Esquerda,
                ExibirCabecalho = ExibirCabecalhoCheckBox.IsChecked == true,
                FonteCabecalho = NormalizarTexto(FonteCabecalhoComboBox.Text, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteCabecalho = LerDouble(TamanhoCabecalhoTextBox.Text, _original.TamanhoFonteCabecalho),
                CabecalhoNegrito = CabecalhoNegritoCheckBox.IsChecked == true,
                CorTextoCabecalho = NormalizarTexto(CorTextoCabecalhoTextBox.Text, ProjectTableDisplaySettings.DefaultHeaderTextColor),
                CorFundoCabecalho = NormalizarTexto(CorFundoCabecalhoTextBox.Text, ProjectTableDisplaySettings.DefaultHeaderBackgroundColor),
                AlturaCabecalho = LerDouble(AlturaCabecalhoTextBox.Text, _original.AlturaCabecalho),
                AlinhamentoCabecalho = AlinhamentoCabecalhoComboBox.SelectedItem is ProjectTableTextAlignment alinhamentoCabecalho ? alinhamentoCabecalho : ProjectTableTextAlignment.Esquerda,
                FonteCorpo = NormalizarTexto(FonteCorpoComboBox.Text, ProjectTableDisplaySettings.DefaultFontFamily),
                TamanhoFonteCorpo = LerDouble(TamanhoCorpoTextBox.Text, _original.TamanhoFonteCorpo),
                CorTextoCorpo = NormalizarTexto(CorTextoCorpoTextBox.Text, ProjectTableDisplaySettings.DefaultBodyTextColor),
                CorFundoCorpo = NormalizarTexto(CorFundoCorpoTextBox.Text, ProjectTableDisplaySettings.DefaultBodyBackgroundColor),
                AlturaLinhaCorpo = LerDouble(AlturaLinhaCorpoTextBox.Text, _original.AlturaLinhaCorpo),
                AlinhamentoCorpo = AlinhamentoCorpoComboBox.SelectedItem is ProjectTableTextAlignment alinhamentoCorpo ? alinhamentoCorpo : ProjectTableTextAlignment.Esquerda,
                UsarLinhasAlternadas = LinhasAlternadasCheckBox.IsChecked == true,
                CorLinhaAlternada = NormalizarTexto(CorLinhaAlternadaTextBox.Text, ProjectTableDisplaySettings.DefaultAlternateRowBackgroundColor),
                ExibirLinhasGrade = ExibirGradeCheckBox.IsChecked == true,
                CorGrade = NormalizarTexto(CorGradeTextBox.Text, ProjectTableDisplaySettings.DefaultGridColor),
                EspessuraGrade = LerDouble(EspessuraGradeTextBox.Text, _original.EspessuraGrade),
                ExibirContornoExterno = ExibirContornoCheckBox.IsChecked == true,
                CorContorno = NormalizarTexto(CorContornoTextBox.Text, ProjectTableDisplaySettings.DefaultOutlineColor),
                EspessuraContorno = LerDouble(EspessuraContornoTextBox.Text, _original.EspessuraContorno)
            };
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